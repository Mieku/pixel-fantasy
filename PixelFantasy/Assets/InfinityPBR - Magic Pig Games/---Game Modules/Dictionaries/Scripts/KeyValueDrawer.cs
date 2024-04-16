namespace InfinityPBR.Modules
{
    public interface KeyValueDrawer
    {
        void DrawBool(KeyValue keyValue, bool structureOnly = false);
        void DrawFloat(KeyValue keyValue, bool structureOnly = false);
        void DrawInt(KeyValue keyValue, bool structureOnly = false);
        void DrawString(KeyValue keyValue, bool structureOnly = false);
        void DrawAnimation(KeyValue keyValue, bool structureOnly = false);
        void DrawTexture2D(KeyValue keyValue, bool structureOnly = false);
        void DrawSprite(KeyValue keyValue, bool structureOnly = false);
        void DrawAudioClip(KeyValue keyValue, bool structureOnly = false);
        void DrawPrefab(KeyValue keyValue, bool structureOnly = false);
        void DrawColor(KeyValue keyValue, bool structureOnly = false);
        void DrawVector3(KeyValue keyValue, bool structureOnly = false);
        void DrawVector2(KeyValue keyValue, bool structureOnly = false);
        void DrawStat(KeyValue keyValue, bool structureOnly = false);
        void DrawItemObject(KeyValue keyValue, bool structureOnly = false);
        void DrawItemObjectType(KeyValue keyValue, bool structureOnly = false);
        void DrawItemAttribute(KeyValue keyValue, bool structureOnly = false);
        void DrawItemAttributeType(KeyValue keyValue, bool structureOnly = false);
        
        void Draw(KeyValue keyValue, bool structureOnly = false);
        void Draw_v3(KeyValue keyValue, bool structureOnly = false);
        void DrawLabel(string name);
        void DrawDetails(KeyValue keyValue, ref bool shouldDraw);

        void SetAllDictionaries(Dictionaries[] allDictionaries);
        bool AllDictionariesIsNull();
    }
}