using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using SKStudios.Common.Editor;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine;

namespace SKStudios.Portals.Editor {
	public class FeedbackPanel : GUIPanel {
		[MenuItem( SettingsWindow.baseMenuPath + "Feedback", priority = 300 )]
		static void Show () {
			SettingsWindow.Show( true, SettingsWindow.FEEDBACK );
		}

		internal static class Content {

			public static readonly GUIContent highRatingPlaceholder = new GUIContent( "Any comments or concerns? Let them be heard." +
				"\n(This is not an anonymous form)" );

			public static readonly GUIContent lowRatingPlaceholder = new GUIContent( "We'd hate to leave anyone with anything less than a five-star \n" +
				"experience. If you submit feedback using this form, we will use \n" +
				"it to improve the asset in future updates.\n" +
				"(This is not an anonymous form)" );


			public static readonly GUIContent ratingPrompt = new GUIContent( "How would you rate PortalKit Pro?" );
			public static readonly GUIContent supportLabel = new GUIContent( "I need help!" );

			public static readonly GUIContent s1Text = new GUIContent( "Send us an email" );
			public static readonly string s1WebsiteLink = "mailto:support@skstudios.zendesk.com";

			public static readonly GUIContent s2Text = new GUIContent( "Open the Documentation" );

			public static readonly GUIContent ReviewText = new GUIContent(
				"I'm glad to hear that you're enjoying the asset! Do you want to leave a review or a rating?" );

			public static readonly GUIContent SupportText = new GUIContent(
				"I'm sorry that you're having a bad experience with " +
				"the asset. If you need it, please contact my support line here:" );

			public static readonly GUIContent SupportLinkText = new GUIContent( "Support Contact" );

			public static readonly GUIContent RatingLinkText = new GUIContent( "Leave a review/rating in the store" );
			public static readonly string RatingLink = "https://www.assetstore.unity3d.com/en/#!/content/81638";

			public static readonly GUIContent GalleryText = new GUIContent(
				"I made something cool! " );
			public static readonly GUIContent GalleryResponseText = new GUIContent(
				"Awesome! If you want, you can submit it to our (wip) gallery of totally awesome projects:" );
			public static readonly GUIContent GalleryLinkText = new GUIContent( "Submit it to our gallery!" );
			public static readonly string GalleryLink = "mailto:superkawaiiltd@gmail.com";

			public static readonly GUIContent submitButton = new GUIContent( "Submit" );
			public static readonly GUIContent submittedButton = new GUIContent( "Sent!" );
		}

		private const int maxRating = 5;
		private const int supportRating = 3;

		//private static readonly float starSizeFull = starSize * maxRating;

		internal static class Styles {
			static bool _initialized = false;

			public static GUIStyle bgStyle;
			public static GUIStyle starSliderStyle;
			public static GUIStyle ratingStyle;
			public static GUIStyle feedbackLabelStyle;
			public static GUIStyle feedbackTextAreaStyle;
			public static GUIStyle feedbackPlaceholderStyle;

			public static GUIStyle emailStyle;
			public static GUIStyle submitStyle;

			public static Color proStarColor = Color.white;
			public static Color nonProStarColor = new Color( 0.27f, 0.27f, 0.27f, 1 );
			public static Color maxRatingStarColor = new Color( 1, 0.61f, 0, 1 );

			static readonly float starSize = EditorGUIUtility.singleLineHeight * 1.5f;

			public static void Init () {

			    var proBg = GlobalStyles.LoadImageResource("pkpro_selector_bg_pro");
			    var defaultBg = GlobalStyles.LoadImageResource("pkpro_selector_bg");


                if ( _initialized ) return;
				_initialized = true;

				bgStyle = new GUIStyle();
                bgStyle.normal.background = EditorGUIUtility.isProSkin ? proBg : defaultBg;
				bgStyle.border = new RectOffset( 2, 2, 2, 2 );

				var starEmptyTex = GlobalStyles.LoadImageResource( "pkpro_rating_star_empty" );
				var starFullTex = GlobalStyles.LoadImageResource( "pkpro_rating_star_filled" );

				ratingStyle = new GUIStyle();
				ratingStyle.normal.background = starEmptyTex;
				ratingStyle.onNormal.background = starFullTex;
				ratingStyle.fixedWidth = starSize;
				ratingStyle.fixedHeight = starSize;
				ratingStyle.padding = new RectOffset( 1, 1, 0, 0 );

				starSliderStyle = new GUIStyle( GUI.skin.horizontalSlider );

				feedbackLabelStyle = new GUIStyle( EditorStyles.label );
				feedbackLabelStyle.richText = true;
				feedbackLabelStyle.wordWrap = true;
				feedbackLabelStyle.alignment = TextAnchor.UpperLeft;

				feedbackTextAreaStyle = new GUIStyle( EditorStyles.textArea );
				feedbackTextAreaStyle.margin = EditorStyles.boldLabel.margin;
				feedbackTextAreaStyle.padding = EditorStyles.boldLabel.padding;

				feedbackPlaceholderStyle = new GUIStyle( EditorStyles.label );
				feedbackPlaceholderStyle.margin = EditorStyles.boldLabel.margin;
				
				emailStyle = new GUIStyle( EditorStyles.textField );
				emailStyle.margin = new RectOffset( 0, 0, 0, 0 );
				emailStyle.alignment = TextAnchor.UpperLeft;

				submitStyle = new GUIStyle( GUI.skin.button );
				submitStyle.margin = new RectOffset( 0, 0, 0, 0 );
            }
		}


		public override string title {
			get {
				return "Feedback";
			}
		}

		bool timedFeedbackPopupMode = false;

		private int rating = -1;
		private bool hasSubmitted = false;

		string feedbackText = "";

		string documentPath;

		private AnimBool fadeRatingPopup;
		private AnimBool fadeSupportPopup;

		AnimBool fadeTimedPopupModeBody;

		SettingsWindow window;

		public FeedbackPanel ( SettingsWindow window ) {
			this.window = window;
			fadeTimedPopupModeBody = new AnimBool( rating != 0, window.Repaint );

			fadeRatingPopup = new AnimBool( rating == maxRating, window.Repaint );
			fadeSupportPopup = new AnimBool( rating > 0 && rating < supportRating, window.Repaint );
		}

		public override void OnEnable () {
			documentPath = SetupUtility.GetDocumentationPath();

			// this is disabled when the window is closed
			if( SetupUtility.timedFeedbackPopupActive ) {
				timedFeedbackPopupMode = true;
			}
		}

		public override void OnGUI ( Rect position ) {
			Styles.Init();

			position = ApplySettingsPadding( position );

			GUILayout.BeginArea( position );
			{
				GUILayout.Label( title, GlobalStyles.settingsHeaderText );

				EditorGUILayout.Space();

				// rating bar
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.Label( Content.ratingPrompt, EditorStyles.boldLabel );
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				var starColor = EditorGUIUtility.isProSkin ? Styles.proStarColor : Styles.nonProStarColor;
				if( rating == maxRating ) {
					starColor = Styles.maxRatingStarColor;
				}

				GlobalStyles.BeginColorArea( starColor );

				for( int i = 0; i < maxRating; i++ ) {
					var starRect = GUILayoutUtility.GetRect( GUIContent.none, Styles.ratingStyle );
					// use manual mouse events in order to trigger on mouse down rather than mouse up
					if( Event.current.type == EventType.Repaint ) {
						Styles.ratingStyle.Draw( starRect, GUIContent.none, false, false, i < rating, false );
					} else if( Event.current.type == EventType.MouseDown ) {
						if( starRect.Contains( Event.current.mousePosition ) ) {
							var oldRating = rating;
							rating = i + 1;
							EditorPrefs.SetInt( "pkpro_feedback_rating", rating );
							UpdateFaders();
							if( timedFeedbackPopupMode ) {
								fadeTimedPopupModeBody.target = true;

								// snap to target on first use
								if( oldRating == 0 ) {
									fadeRatingPopup.value = fadeRatingPopup.target;
									fadeSupportPopup.value = fadeSupportPopup.target;
								}
							}
							Event.current.Use();
							window.Repaint();
						}
					}

					EditorGUIUtility.AddCursorRect( starRect, MouseCursor.Link );
				}

				GlobalStyles.EndColorArea();

				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();

				if( timedFeedbackPopupMode ) {
					GlobalStyles.BeginColorArea( Color.Lerp( new Color( 1, 1, 1, 0 ), Color.white, fadeTimedPopupModeBody.faded ) );
				}

				
				GUILayout.Space( 5 );

				//Rating Dialog
				if( EditorGUILayout.BeginFadeGroup( fadeRatingPopup.faded ) ) {
					EditorGUILayout.LabelField( Content.ReviewText, Styles.feedbackLabelStyle );
					GlobalStyles.LayoutExternalLink( Content.RatingLinkText, Content.RatingLink );
				}
				EditorGUILayout.EndFadeGroup();



				//Support dialog
				if( EditorGUILayout.BeginFadeGroup( fadeSupportPopup.faded ) ) {
					EditorGUILayout.LabelField( Content.SupportText, Styles.feedbackLabelStyle );
					GlobalStyles.LayoutExternalLink( Content.SupportLinkText, Content.s1WebsiteLink );
				}
				EditorGUILayout.EndFadeGroup();

				//Feedback Email
				//Feedback input box


				GUI.enabled = !hasSubmitted;
				{
					float minHeight = EditorGUIUtility.singleLineHeight * 4;

					// @Hack to get textarea to properly scale with text
					var textRect = GUILayoutUtility.GetRect( 0, 999, 0, 0 );
					textRect.height = Styles.feedbackTextAreaStyle.CalcHeight( new GUIContent( feedbackText ), textRect.width );

					textRect = GUILayoutUtility.GetRect( textRect.width, textRect.height, Styles.feedbackTextAreaStyle, GUILayout.MinHeight( minHeight ) );

					feedbackText = EditorGUI.TextArea( textRect, feedbackText, Styles.feedbackTextAreaStyle );

					if( string.IsNullOrEmpty( feedbackText ) ) {
						GlobalStyles.BeginColorArea( new Color( 1, 1, 1, GUI.color.a * 0.6f ) );

						var placeholderContent = rating == maxRating ? Content.highRatingPlaceholder : Content.lowRatingPlaceholder;
						GUI.Label( textRect, placeholderContent, Styles.feedbackPlaceholderStyle );

						GlobalStyles.EndColorArea();
					}

					var buttonContent = hasSubmitted ? Content.submittedButton : Content.submitButton;

					if( GUILayout.Button( buttonContent ) ) {
						hasSubmitted = true;
						CreateAndSendFeedbackReport();
					}
				}
				GUI.enabled = true;

				GUILayout.FlexibleSpace();
				EditorGUILayout.Space();

				//Content submission
				GUILayout.Label( Content.GalleryText, EditorStyles.boldLabel );
				GUILayout.Label( Content.GalleryResponseText, Styles.feedbackLabelStyle );
				GlobalStyles.LayoutExternalLink( Content.GalleryLinkText, Content.GalleryLink );

				EditorGUILayout.EndFadeGroup();

				EditorGUILayout.Space();

				GUILayout.Label( Content.supportLabel, EditorStyles.boldLabel );
				GlobalStyles.LayoutExternalLink( Content.s1Text, Content.s1WebsiteLink );
				if( !string.IsNullOrEmpty( documentPath ) ) {
					GlobalStyles.LayoutExternalLink( Content.s2Text, documentPath );
				}
			}


			if( timedFeedbackPopupMode ) {
				GlobalStyles.EndColorArea();
			}

			GUILayout.EndArea();
		}

		private void UpdateFaders () {
			//Show the rating dialog if rating is full
			fadeRatingPopup.target = rating == maxRating;
			//Show the support dialog if rating is less than the support level
			fadeSupportPopup.target = rating < supportRating;
		}

		void CreateAndSendFeedbackReport () {

			/* Thanks for keeping us honest!
			 * We only collect information that we think will assist our assessment of any and all 
			 * feedback samples. Information gathered is effectively anonymous (we can only tell if
			 * a machine has submitted multiple tickets, not who owns the machine or where it is)
			 */

			var feedbackBuilder = new StringBuilder();

			var prettyVersionString = String.Format(
				"{0}.{1}.{2}",
				GlobalPortalSettings.MAJOR_VERSION, 
				GlobalPortalSettings.MINOR_VERSION,
				GlobalPortalSettings.PATCH_VERSION
			);

			var dataToInclude = new object[] {
				
				Application.unityVersion,
				prettyVersionString,

			    SKSGlobalRenderSettings.Instance.ToString(),
			    GlobalPortalSettings.Instance.ToString(),

			    SystemInfo.deviceUniqueIdentifier,
                SystemInfo.graphicsDeviceName,
				SystemInfo.operatingSystem,
				SystemInfo.processorType,
				SystemInfo.systemMemorySize,
				SystemInfo.graphicsMemorySize,

                Application.platform,
				EditorUserBuildSettings.activeBuildTarget,
				SetupUtility.projectMode,
				Application.systemLanguage
			};

			feedbackBuilder.AppendFormat( "{0}|", LazySanitize( feedbackText ) );
			for( int i = 0; i < maxRating; i++ ) {
				feedbackBuilder.Append( i < rating ? '★' : '☆' );
			}

			for( int i = 0; i < dataToInclude.Length; i++ ) {
				feedbackBuilder.AppendFormat( "|{0}", dataToInclude[ i ].ToString() );
			}

			var feedbackString = feedbackBuilder.ToString();
			// Queue and send IRC message (wow such technology (I swear there is a reason we're doing it this way))
			TcpClient socket = new TcpClient();
			Int32 port = 7000;
			string server = "irc.rizon.net";
			String chan = "#SKSPortalKitFeedback";
			socket.Connect( server, port );
			StreamReader input = new System.IO.StreamReader( socket.GetStream() );
			StreamWriter output = new System.IO.StreamWriter( socket.GetStream() );
			String nick = SystemInfo.deviceName;
			output.Write(
				"USER " + nick + " 0 * :" + "ChunkBuster" + "\r\n" +
				"NICK " + nick + "\r\n"
			);
			output.Flush();
			Thread sendMsgThread = new Thread( () => {
				while( true ) {
					try {
						String buf = input.ReadLine();
						if( buf == null ) {
							return;
						}

						//Send pong reply to any ping messages
						if( buf.StartsWith( "PING " ) ) {
							output.Write( buf.Replace( "PING", "PONG" ) + "\r\n" );
							output.Flush();
						}
						if( buf[ 0 ] != ':' ) continue;

						/* IRC commands come in one of these formats:
						 * :NICK!USER@HOST COMMAND ARGS ... :DATA\r\n
						 * :SERVER COMAND ARGS ... :DATA\r\n
						 */

						//After server sends 001 command, we can set mode to bot and join a channel
						if( buf.Split( ' ' )[ 1 ] == "001" ) {
							output.Write(
								"MODE " + nick + " +B\r\n" +
								"JOIN " + chan + "\r\n"
							);
							output.Flush();
							continue;
						}
						if( buf.Contains( "End of /NAMES list" ) ) {
							String[] outputText = feedbackString.Split( '\n' );
							foreach( string s in outputText ) {
								output.Write( "PRIVMSG " + chan + " :" + s + "\r\n" );
								output.Flush();
								Thread.Sleep( 200 );
							}

							socket.Close();
							return;
						}
					} catch( Exception ) {
						//If this doesn't function perfectly then just dispose of it, not worth possibly causing issues with clients over network issues
						return;
					}
				}
			} );
			sendMsgThread.Start();
		}

		string LazySanitize ( string s ) {
			return s.Replace( Environment.NewLine, " " ).Replace( "\n", " " ).Replace( "|", " " );
		}
	}
}