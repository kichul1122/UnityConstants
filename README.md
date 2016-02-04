# Unity Constants

Based on UnityToolbag by [Nick Gravelyn](https://github.com/nickgravelyn/UnityToolbag/tree/master/UnityConstants).

UnityConstants generates a C# script containing various Unity magic strings. Currently it works with:

* Tags
* Sorting layers
* Layers
* Scenes
* Inputs
* Audio mixer parameters
* Animator controller parameters

It will generate a file similar to this:

    namespace UnityConstants
    {
        public static class Tags
        {
            public const string Untagged = "Untagged";
            public const string MainCamera = "MainCamera";
            public const string Player = "Player";
        }

        public static class SortingLayers
        {
            public const int Default = 0;
            public const int Foreground = 7;
        }

        public static class Layers
        {
            public const int Default = 0;
            public const int TransparentFX = 1;
            public const int IgnoreRaycast = 2;
        }

        public static class Scenes
        {
            public const int MainMenu = 0;
            public const int Tutorial = 1;
        }
    }
