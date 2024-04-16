using System;
using System.Collections.Generic;
using System.Linq;

namespace InfinityPBR.Modules
{
    public static class GameModuleListExtensions
    {
        /* --------------------------------------------------------------------------- */
        // Get & Try Get
        /* --------------------------------------------------------------------------- */
        
        public static IEnumerable<T> GetAllOfType<T>(this GameModulesList<T> gameModulesList, string objectType) where T : IAmGameModuleObject
            => gameModulesList.list.Where(x => x.ObjectType() == objectType);
        
        public static IEnumerable<T> GetAll<T>(this GameModulesList<T> gameModulesList, string uid) where T : IAmGameModuleObject
            => gameModulesList.list.Where(x => x.Uid() == uid);
        
        public static bool TryGet<T>(this GameModulesList<T> gameModulesList, string uid, out T found) where T : IAmGameModuleObject
        {
            found = gameModulesList.list.FirstOrDefault(x => x.Uid() == uid);
            return found != null;
        }
    
        public static bool TryGet<T>(this GameModulesList<T> gameModulesList, T gameModuleObject, out T found) where T : IAmGameModuleObject
            => gameModulesList.TryGet(gameModuleObject.Uid(), out found);
    
        public static bool TryGetGameId<T>(this GameModulesList<T> gameModulesList, string gameId, out T found) where T : IAmGameModuleObject
        {
            found = gameModulesList.GetByGameId(gameId);
            return found != null;
        }
    
        public static bool TryGetByObjectType<T>(this GameModulesList<T> gameModulesList, string objectType, out T found) where T : IAmGameModuleObject
        {
            found = gameModulesList.GetByObjectType(objectType);
            return found != null;
        }
        
        public static T GetByObjectType<T>(this GameModulesList<T> gameModulesList, string objectType) where T : IAmGameModuleObject
            => gameModulesList.list.FirstOrDefault(x => x.ObjectType() == objectType);
        
        public static T Get<T>(this GameModulesList<T> gameModulesList, T gameModuleObject, bool addIfNull = false, bool distinct = false) where T : IAmGameModuleObject
        {
            if (gameModulesList.TryGet(gameModuleObject.Uid(), out var found))
                return found;

            // Assuming the Add method will be called on the list itself:
            return addIfNull ? gameModulesList.Add(gameModuleObject.Uid(), distinct) : default;
        }
    
        public static T Get<T>(this GameModulesList<T> gameModulesList, string uid, bool addIfNull = false, bool distinct = false) where T : IAmGameModuleObject
        {
            if (gameModulesList.TryGet(uid, out var found))
                return found;

            // Assuming the Add method will be called on the list itself:
            return addIfNull ? gameModulesList.Add(uid, distinct) : default;
        }
        
        public static T Last<T>(this GameModulesList<T> gameModulesList) where T : IAmGameModuleObject 
            => gameModulesList.list[^1];

        public static T First<T>(this GameModulesList<T> gameModulesList) where T : IAmGameModuleObject
            => gameModulesList.list[0];
    
        public static T GetByGameId<T>(this GameModulesList<T> gameModulesList, string gameId) where T : IAmGameModuleObject 
            => gameModulesList.list.FirstOrDefault(x => x.GameId() == gameId);
    
        public static IEnumerable<T> GetUnique<T>(this GameModulesList<T> gameModulesList, bool takeFirst = true) where T : IAmGameModuleObject 
            => gameModulesList.list
                .GroupBy(x => x.Uid())
                .Select(g => takeFirst ? g.First() : g.Last());

        public static IEnumerable<T> GetObjectTypes<T>(this GameModulesList<T> gameModulesList, bool takeFirst = true) where T : IAmGameModuleObject 
            => gameModulesList.list
                .GroupBy(x => x.ObjectType())
                .Select(g => takeFirst ? g.First() : g.Last());
    
        public static IEnumerable<T> GetDuplicates<T>(this GameModulesList<T> gameModulesList, string uid = null) where T : IAmGameModuleObject
        {
            var items = uid == null
                ? gameModulesList.list
                : gameModulesList.list.Where(x => x.Uid() == uid);

            return items
                .GroupBy(x => x.Uid())
                .SelectMany(g => g.Skip(1));
        }

        public static IEnumerable<T> GetDuplicatesOfType<T>(this GameModulesList<T> gameModulesList, string objectType) where T : IAmGameModuleObject =>
            gameModulesList.list
                .Where(x => x.ObjectType() == objectType)
                .GroupBy(x => x.ObjectType())
                .SelectMany(g => g.Skip(1));
        
        /* --------------------------------------------------------------------------- */
        // Contains
        /* --------------------------------------------------------------------------- */
        
        public static bool Contains<T>(this GameModulesList<T> gameModulesList, T item) where T : IAmGameModuleObject
            => gameModulesList.list.Any(x => x.Uid() == item.Uid());

        public static bool Contains<T>(this GameModulesList<T> gameModulesList, string uid) where T : IAmGameModuleObject
            => gameModulesList.list.Any(x => x.Uid() == uid);

        public static bool ContainsGameId<T>(this GameModulesList<T> gameModulesList, string gameId) where T : IAmGameModuleObject
            => gameModulesList.list.Any(x => x.GameId() == gameId);

        public static bool ContainsName<T>(this GameModulesList<T> gameModulesList, string objectName) where T : IAmGameModuleObject
            => gameModulesList.list.Any(x => x.ObjectName() == objectName);
        public static bool ContainsObjectType<T>(this GameModulesList<T> gameModulesList, string objectType) where T : IAmGameModuleObject
            => gameModulesList.list.Any(x => x.ObjectType() == objectType);
        
        public static bool ContainsDuplicates<T>(this GameModulesList<T> gameModulesList, string uid = null) where T : IAmGameModuleObject 
            => uid == null
                ? gameModulesList.list.GroupBy(x => x.Uid()).Any(g => g.Count() > 1)
                : gameModulesList.list.Count(x => x.Uid() == uid) > 1;


        /* --------------------------------------------------------------------------- */
        // Count
        /* --------------------------------------------------------------------------- */
        
        public static int CountObjectType<T>(this GameModulesList<T> gameModulesList, string objectType) where T : IAmGameModuleObject
            => gameModulesList.list.Count(x => x.ObjectType() == objectType);

        public static int Count<T>(this GameModulesList<T> gameModulesList, string uid = null) where T : IAmGameModuleObject
            => uid == null ? gameModulesList.list.Count : gameModulesList.list.Count(x => x.Uid() == uid);

        public static int CountUnique<T>(this GameModulesList<T> gameModulesList) where T : IAmGameModuleObject
            => gameModulesList.list.GroupBy(x => x.Uid()).Count();
            
        public static int CountTypes<T>(this GameModulesList<T> gameModulesList) where T : IAmGameModuleObject
            => gameModulesList.list.GroupBy(x => x.ObjectType()).Count();

        public static int CountDuplicates<T>(this GameModulesList<T> gameModulesList, string uid = null) where T : IAmGameModuleObject
        {
            var items = uid == null ? gameModulesList.list : gameModulesList.list.Where(x => x.Uid() == uid);
            return items.GroupBy(x => x.Uid()).Sum(g => Math.Max(0, g.Count() - 1));
        }

        public static int CountDuplicatesOfType<T>(this GameModulesList<T> gameModulesList, string objectType) where T : IAmGameModuleObject
        {
            var itemsOfType = gameModulesList.list.Where(x => x.ObjectType() == objectType);
            return itemsOfType.GroupBy(x => x.Uid()).Sum(g => Math.Max(0, g.Count() - 1));
        }
        
        /* --------------------------------------------------------------------------- */
        // Remove
        /* --------------------------------------------------------------------------- */

        public static void RemoveAt<T>(this GameModulesList<T> gameModulesList, int index) where T : IAmGameModuleObject
        {
            gameModulesList.SetAffectedStatsDirty(gameModulesList.list[index]);  // You'll have to define this method
            gameModulesList.list.RemoveAt(index);
            gameModulesList.AddThisToBlackboard();  // You'll have to define this method
        }
        
        public static void RemoveType<T>(this GameModulesList<T> gameModulesList, string objectType) where T : IAmGameModuleObject 
            => gameModulesList.list.RemoveAll(x => x.ObjectType() == objectType);

        public static void Remove<T>(this GameModulesList<T> gameModulesList, T item) where T : IAmGameModuleObject
            => gameModulesList.Remove(item.Uid());

        public static void Remove<T>(this GameModulesList<T> gameModulesList, string uid) where T : IAmGameModuleObject
        {
            var found = gameModulesList.list.FirstOrDefault(x => x.Uid() == uid);
            if (found == null)
                return;

            gameModulesList.SetAffectedStatsDirty(found);  // You'll have to define this method
            gameModulesList.list.Remove(found);
            gameModulesList.AddThisToBlackboard();  // You'll have to define this method
        }

        public static void RemoveAll<T>(this GameModulesList<T> gameModulesList, T item) where T : IAmGameModuleObject
            => gameModulesList.RemoveAll(item.Uid());

        public static void RemoveAll<T>(this GameModulesList<T> gameModulesList, string uid) where T : IAmGameModuleObject
        {
            foreach (var item in gameModulesList.list.Where(x => x.Uid() == uid))
                gameModulesList.SetAffectedStatsDirty(item);  // You'll have to define this method

            gameModulesList.list.RemoveAll(x => x.Uid() == uid);
            gameModulesList.AddThisToBlackboard();  // You'll have to define this method
        }

        public static void RemoveGameId<T>(this GameModulesList<T> gameModulesList, string gameId) where T : IAmGameModuleObject
        {
            if (!gameModulesList.TryGetGameId(gameId, out var found))
                return;
            
            gameModulesList.RemoveExact(found);
        }
        
        public static void RemoveExact<T>(this GameModulesList<T> gameModulesList, string gameId) where T : IAmGameModuleObject
        {
            if (!gameModulesList.TryGetGameId(gameId, out var found))
                return;

            gameModulesList.RemoveExact(found);
        }
        
        public static void RemoveExact<T>(this GameModulesList<T> gameModulesList, T gameModuleObject) where T : IAmGameModuleObject
        {
            gameModulesList.SetAffectedStatsDirty(gameModuleObject);
            gameModulesList.list.Remove(gameModuleObject);
            gameModulesList.AddThisToBlackboard();
        }

        public static void Clear<T>(this GameModulesList<T> gameModulesList) where T : IAmGameModuleObject 
            => gameModulesList.list.Clear();
        
        /* --------------------------------------------------------------------------- */
        // Other
        /* --------------------------------------------------------------------------- */
        
        public static void AddThisToBlackboard<T>(this GameModulesList<T> gameModulesList, bool allowNotification = true) where T : IAmGameModuleObject
        {
            if (!gameModulesList.postListToBlackboard) return;
            var note = new BlackboardNote(gameModulesList.GameId(), gameModulesList.NoteSubject(), new object[] { gameModulesList });
            MainBlackboard.blackboard.AddNote(note, allowNotification && gameModulesList.notifyOnPost);
        }
    }
}

