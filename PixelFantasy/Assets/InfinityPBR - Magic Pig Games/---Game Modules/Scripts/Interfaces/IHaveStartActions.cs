using System.Collections;

/*
 * This Interface implies that the object has some sort of StartActions, which are in a coroutine. Often this means
 * there is various data that needs to be set up, or linked at runtime. We do this in a coroutine in case there are
 * Singleton or other actions which have to take place first. This way, we can start the StartActions() coroutine at
 * the start, but it doesn't have to perform it actions until the required Singletons and other criteria are met.
 */

namespace InfinityPBR.Modules
{
    public interface IHaveStartActions
    {
        public IEnumerator StartActions();
    }
}