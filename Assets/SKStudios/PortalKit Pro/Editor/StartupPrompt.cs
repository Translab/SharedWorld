using SKStudios.Common.Editor;
using UnityEngine;
using UnityEditor;
using System;

namespace SKStudios.Portals.Editor {

	public class SettingsWindow : EditorWindow {

		public const int SETUP = 0;
		public const int GENERAL_SETTINGS = 1;
		public const int INTERACTION_SETTINGS = 2;
		public const int EDITOR_SETTINGS = 3;
		public const int FEEDBACK = 4;
		public const int DEBUG = 5;
		public const int ABOUT = 6;

		internal static class Styles {
			static bool _initialized;
			
			public static GUIStyle sectionElement = "PreferencesSection";
			public static GUIStyle selected = "ServerUpdateChangesetOn";
			public static GUIStyle sidebarBg = "PreferencesSectionBox";

			public static GUIStyle divider;

			public static readonly Color dividerColor = EditorGUIUtility.isProSkin ? new Color( 0.157f, 0.157f, 0.157f ) : new Color( 0.5f, 0.5f, 0.5f );

			public static void Init () {
				if( _initialized ) return;
				_initialized = true;

				sidebarBg.padding.bottom = 10;

				divider = new GUIStyle();
				divider.normal.background = EditorGUIUtility.whiteTexture;
				divider.stretchWidth = true;
			}
		}

		const string panelPrefPath = "pkpro_active_settings_panel";

		public const string baseMenuPath = "Tools/SK Studios/PortalKit Pro/";
		public const string menuPath = baseMenuPath + "Setup";

		bool manualClose;

		public ProjectMode selectedMode;

		FirstRunPanel firstRunPanel;

		GUIPanel[] panels;

		int[] dividers;

		GUIPanel activePanel;

		int activePanelIndex = 0;

		[MenuItem( menuPath, priority = 0 )]
		static void ShowFromMenu () {
			Show( true, SETUP );
		}

		public static void Hide ( bool manualClose = false ) {

			// are we already focused?
			var window = focusedWindow as SettingsWindow;

			if( window == null ) {
				// check if window exists anywhere
				FocusWindowIfItsOpen<SettingsWindow>();

				window = focusedWindow as SettingsWindow;

				if( window != null ) {
					// create a new window
					if( window.manualClose ) {
						window.Close();
						window = GetWindow( true );
					} else {
						window.Close();
					}
				}
			} else {
				// window found, but it's probably hidden, and we can't do anything about that... force it open again!
				if( window.manualClose ) {
					window.Close();
					window = GetWindow( true );
				} else {
					window.Close();
				}
			}
		}

		public static void Show ( bool manualClose = false, int openPanelIndex = -1 ) {

			// are we already focused?
			var window = focusedWindow as SettingsWindow;

			if( window == null ) {
				// check if window exists anywhere
				FocusWindowIfItsOpen<SettingsWindow>();

				window = focusedWindow as SettingsWindow;

				if( window == null ) {
					// create a new window
					window = GetWindow( manualClose );
				}
			} else {
				// window found, but it's probably hidden, and we can't do anything about that... force it open again!
				window.Close();
				window = GetWindow( manualClose );
			}

			if( openPanelIndex > -1 ) window.ShowPanel( openPanelIndex );
		}

		private static SettingsWindow GetWindow ( bool manualClose ) {
			SettingsWindow window = CreateInstance<SettingsWindow>();
			window.titleContent = new GUIContent( "PortalKit PRO Settings" );
			window.ShowUtility();
			window.manualClose = manualClose;
			return window;
		}

		void OnEnable () {
			bool ignoringSetup = SetupUtility.ignoringInitialSetup;

			selectedMode = SetupUtility.projectMode;
			
			if( ignoringSetup ) {
				selectedMode = ProjectMode.Default;
			}

			if( !SetupUtility.projectInitialized && firstRunPanel == null ) firstRunPanel = new FirstRunPanel( this );

			if( panels == null ) {
				panels = new GUIPanel[] {
					new SetupPanel( this ),
					new ImageSettingsPanel( this ),
					new InteractionSettingsPanel( this ),
					new EditorSettingsPanel(),
                    new FeedbackPanel( this ),
					new DebugInfoPanel( this ),
					new AboutPanel(),
				};

				dividers = new[] { 1, 0, 0, 1, 0, 1, 0 };
			}

			activePanelIndex = EditorPrefs.GetInt( panelPrefPath, 0 );

			UpdateEnabledPanel();

			if( SetupUtility.projectInitialized ) {
				maxSize = minSize = new Vector2( 560, 480 );
			} else {
				maxSize = minSize = new Vector2( 680, 600 );
			}

			Undo.undoRedoPerformed += OnUndoRedo;
		}

		void OnUndoRedo () {
			Repaint();
		}

		void OnDisable () {
			if( activePanel != null ) activePanel.OnDisable();

			SetupUtility.timedFeedbackPopupActive = false;
			EditorUtility.UnloadUnusedAssetsImmediate();
			Undo.undoRedoPerformed -= OnUndoRedo;
		}

		void OnGUI () {
			GlobalStyles.Init();

			if( SetupUtility.projectInitialized ) {
				DoSettingsGUI();
			} else {
				firstRunPanel.OnGUI( position );
			}

			//if( GUI.Button( new Rect( 0, position.height - 20, position.width, 20 ), "clear data" ) ) {
			//	SetupUtility.DEBUG_ClearSetupData();
			//}
		}

		public void ShowPanel ( int panelIndex ) {
			activePanelIndex = panelIndex;
			UpdateEnabledPanel();
		}

		void DoSettingsGUI () {
			Styles.Init();

			var tabRect = new Rect( 0, 0, 120, position.height );

			// tab select
			tabRect.y -= 1;
			tabRect.height += 2;


			var current = Event.current;
			if( current.type == EventType.KeyDown && GUIUtility.keyboardControl == 0 ) {
				if( current.keyCode == KeyCode.UpArrow ) {
					activePanelIndex--;
					current.Use();
					UpdateEnabledPanel();
				} else if( current.keyCode == KeyCode.DownArrow ) {
					activePanelIndex++;
					current.Use();
					UpdateEnabledPanel();
				}
			}


			GUILayout.BeginArea( tabRect, GUIContent.none, Styles.sidebarBg );

			GUILayout.Space( 40 );

			for( int i = 0; i < panels.Length; i++ ) {
				var panel = panels[ i ];

				Rect rect = GUILayoutUtility.GetRect( new GUIContent( panel.title ), Styles.sectionElement, new GUILayoutOption[] {
					GUILayout.ExpandWidth(true)
				});

				if( i == activePanelIndex && Event.current.type == EventType.Repaint ) {
					Styles.selected.Draw( rect, false, false, false, false );
				}

				EditorGUI.BeginChangeCheck();
				if( GUI.Toggle( rect, i == activePanelIndex, panel.title, Styles.sectionElement ) ) {
					activePanelIndex = i;
				}

				if( dividers[ i ] == 1 ) {
					GUILayout.Space( 10 );

					var r = GUILayoutUtility.GetRect( GUIContent.none, Styles.divider, GUILayout.Height( 1 ) );

					if( Event.current.type == EventType.Repaint ) {
						var c = GUI.color;
						GUI.color = Styles.dividerColor;
						Styles.divider.Draw( r, false, false, false, false );
						GUI.color = c;
					}

					GUILayout.Space( 10 );
				}

				if( EditorGUI.EndChangeCheck() ) {
					UpdateEnabledPanel();
					GUIUtility.keyboardControl = 0;
				}
			}

			GUILayout.EndArea();

			// draw active panel
			if( activePanel != null ) {
				var panelRect = new Rect( tabRect.width + 1, 0, position.width - tabRect.width - 1, position.height );
				GUILayout.BeginArea( panelRect );
				panelRect.x = 0;
				panelRect.y = 0;
				activePanel.OnGUI( panelRect );
				GUILayout.EndArea();
			}
		}

		private void UpdateEnabledPanel () {
			if( activePanelIndex < 0 ) activePanelIndex = 0;
			if( activePanelIndex > panels.Length - 1 ) activePanelIndex = panels.Length - 1;

			if( activePanel != null ) activePanel.OnDisable();
			activePanel = panels[ activePanelIndex ];

			activePanel.OnEnable();

			EditorPrefs.SetInt( panelPrefPath, activePanelIndex );
		}
	}
}