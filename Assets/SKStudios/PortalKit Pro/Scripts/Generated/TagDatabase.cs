using System.Collections.Generic;

namespace SKStudios.Portals {
    public static class TagDatabase {
        public static Dictionary<string, Dictionary<string, string>> tags =
            new Dictionary<string, Dictionary<string, string>> {
                {"Fallback", new Dictionary<string, string>()},
                {"HTCVive", new Dictionary<string, string>()},
                {"OculusTouch_Left", new Dictionary<string, string>()},
                {"OculusTouch_Right", new Dictionary<string, string>()},
                {"Simulator", new Dictionary<string, string>()},
                {"SteamVROculusTouch_Left", new Dictionary<string, string>()},
                {"SteamVROculusTouch_Right", new Dictionary<string, string>()},
                {"XimmerseCobra02", new Dictionary<string, string>()},
                {"InverseBox", new Dictionary<string, string>()},
                {"PortalEffect", new Dictionary<string, string>()}, {
                    "InvisiblePortal",
                    new Dictionary<string, string> {
                        {"PortalTarget", "CustomRenderer"},
                        {"PortalSource", "CustomRenderer"},
                        {"PortalCamera", "CustomRenderer"},
                        {"PortalCameraParent", "CustomRenderer"},
                        {"PortalPhysicsPassthrough", "CustomRenderer"},
                        {"PortalTrigger", "CustomRenderer"},
                        {"BackWall", "CustomRenderer"},
                        {"PortalEdgeWall", "CustomRenderer"},
                        {"PortalPlaceholder", "CustomRendererPlaceholder"},
                        {"PortalRenderer", "CustomRenderer"},
                        {"PortalOpeningSize", "CustomRenderer"},
                        {"SeamlessRecursionFix", "CustomRendererOnly"},
                        {"InvisiblePortal", "CustomRenderer"}
                    }
                }, {
                    "RectPortal",
                    new Dictionary<string, string> {
                        {"FloorSegment", "CustomRenderer"},
                        {"PortalTarget", "CustomRenderer"},
                        {"PortalSource", "CustomRenderer"},
                        {"PortalCamera", "CustomRenderer"},
                        {"PortalCameraParent", "CustomRenderer"},
                        {"Physics Passthrough Scanner", "CustomRenderer"},
                        {"PortalPhysicsPassthrough", "CustomRenderer"},
                        {"PortalTrigger", "CustomRenderer"},
                        {"BackWall", "CustomRenderer"},
                        {"PortalPlaceholder", "CustomRendererPlaceholder"},
                        {"EdgeWall", "CustomRenderer"},
                        {"EdgeWall 2", "CustomRenderer"},
                        {"SeamlessRecursionFix", "CustomRendererOnly"},
                        {"EdgeWall 3", "CustomRenderer"},
                        {"EdgeWall 4", "CustomRenderer"},
                        {"EdgeWall 4 (1)", "CustomRenderer"},
                        {"PortalEdgeWall", "CustomRenderer"},
                        {"PortalRenderer", "CustomRenderer"},
                        {"PortalOpeningSize", "CustomRenderer"},
                        {"RectPortal", "CustomRenderer"}
                    }
                }, {
                    "CirclePortal",
                    new Dictionary<string, string> {
                        {"PortalTarget", "CustomRenderer"},
                        {"PortalSource", "CustomRenderer"},
                        {"PortalCamera", "CustomRenderer"},
                        {"PortalCameraParent", "CustomRenderer"},
                        {"Physics Passthrough Scanner", "CustomRenderer"},
                        {"PortalPhysicsPassthrough", "CustomRenderer"},
                        {"PortalTrigger", "CustomRenderer"},
                        {"BackWall", "CustomRenderer"},
                        {"PortalPlaceholder", "CustomRendererPlaceholder"},
                        {"SeamlessRecursionFix", "CustomRendererOnly"},
                        {"EdgeWall", "CustomRenderer"},
                        {"EdgeWall 2", "CustomRenderer"},
                        {"EdgeWall 3", "CustomRenderer"},
                        {"EdgeWall 4", "CustomRenderer"},
                        {"EdgeWall 4 (1)", "CustomRenderer"},
                        {"PortalEdgeWall", "CustomRenderer"},
                        {"PortalRenderer", "CustomRenderer"},
                        {"PortalOpeningSize", "CustomRenderer"},
                        {"CirclePortal", "CustomRenderer"}
                    }
                }, {
                    "RectPortal NoBuffer",
                    new Dictionary<string, string> {
                        {"PortalTarget", "CustomRenderer"},
                        {"PortalSource", "CustomRenderer"},
                        {"PortalCamera", "CustomRenderer"},
                        {"PortalCameraParent", "CustomRenderer"},
                        {"Physics Passthrough Scanner", "CustomRenderer"},
                        {"PortalPhysicsPassthrough", "CustomRenderer"},
                        {"PortalTrigger", "CustomRenderer"},
                        {"BackWall", "CustomRenderer"},
                        {"PortalPlaceholder", "CustomRendererPlaceholder"},
                        {"PortalEdgeWall", "CustomRenderer"},
                        {"SeamlessRecursionFix", "CustomRendererOnly"},
                        {"PortalRenderer", "CustomRenderer"},
                        {"PortalOpeningSize", "CustomRenderer"},
                        {"RectPortalNoBuffer", "CustomRenderer"}
                    }
                },
                {"PortalSpawner", new Dictionary<string, string> {{"PortalSpawner", "CustomRenderer"}}},
                {"[CameraRig]", new Dictionary<string, string>()},
                {"PortalKitPro VRTK Starter", new Dictionary<string, string> {{"Camera (eye)", "Player"}}},
                {"Arc", new Dictionary<string, string>()}
            };
    }
}