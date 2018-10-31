using System ;
using System . Collections ;
using System . Collections . Generic ;
using System . Diagnostics ;
using System . Linq ;
using System . Net ;
using System . Text ;

using DreamRecorder . ToolBox . CommandLine ;
using DreamRecorder . ToolBox . General ;

using Microsoft . Extensions . Logging ;

using Telegram . Bot ;
using Telegram . Bot . Args ;
using Telegram . Bot . Types ;
using Telegram . Bot . Types . Enums ;

using WenceyWang . FIGlet ;

namespace WenceyWang . AntiPlaneWuBot
{

	public class Program : ProgramBase <Program , ProgramExitCode , ProgramSetting , ProgramSettingCatalog>
	{

		public TelegramBotClient BotClient { get ; set ; }

		public override bool WaitForExit => true ;

		public override string License => "See https://www.gnu.org/licenses/agpl-3.0.en.html" ;

		public override bool CanExit => false ;

		public override bool HandleInput => false ;

		public override bool LoadSetting => true ;

		public override bool AutoSaveSetting => true ;

		private List <MarkedValue <string , int>> EmojiList { get ; set ; }


		public override async void Start ( string [ ] args )
		{
			WebProxy webProxy ;
			if ( Setting . HttpProxy != null )
			{
				webProxy = new WebProxy ( Setting . HttpProxy ) ;
			}
			else
			{
				webProxy = new WebProxy ( ) ;
			}

			BotClient = new TelegramBotClient ( Setting . BotToken , webProxy ) ;

			Logger . LogInformation ( "Connecting..." ) ;

			User me = await BotClient . GetMeAsync ( ) ;

			Logger . LogInformation ( "Use Bot {0}" , me . Username ) ;
			Console . Title = me . Username ;

			BotClient . OnMessage += BotClient_OnMessage ;

			EmojiList = Emojis . EmojisList . Values .
								Select ( emoji => new MarkedValue <string , int> ( emoji , 0 ) ) .
								ToList ( ) ;

			EmojiList . Sort ( ( x , y ) => y . Value . Length - x . Value . Length ) ;

			BotClient . StartReceiving ( ) ;
		}

		private void BotClient_OnMessage ( object sender , MessageEventArgs e )
		{
			Message message = e ? . Message ;

			if ( message != null &&
				message . Type == MessageType . Text )
			{
				Stopwatch stopwatch = new Stopwatch ( ) ;

				stopwatch . Start ( ) ;

				List <MarkedValue <string , int>> emojiList =
					new List <MarkedValue <string , int>> (
						EmojiList . Select (
							emoj => new MarkedValue <string , int> ( emoj . Value , emoj . Mark ) ) ) ;
				string text = e . Message . Text . Normalize ( NormalizationForm . FormD ) ;
				int lenth = text . Length ;

				int emojiLenth = 0 ;
				int nonEmojiLenth = 0 ;


				while ( emojiLenth + nonEmojiLenth < lenth )
				{
					MarkedValue <string , int> emoji =
						emojiList . FirstOrDefault ( emoj =>
														text . StartsWith ( emoj , StringComparison . Ordinal ) ) ;

					if ( emoji != null )
					{
						emojiLenth += emoji . Value . Length ;
						emoji . Mark++ ;
						text = text . Substring ( emoji . Value . Length ) ;
					}
					else
					{
						nonEmojiLenth++ ;
						text = text . Substring ( 1 ) ;
					}
				}

				int usedEmojiTypeCount = 0 ;

				int amountOfEmoji = 0 ;

				foreach ( MarkedValue <string , int> emoji in emojiList )
				{
					if ( emoji . Mark > 0 )
					{
						usedEmojiTypeCount++ ;
						amountOfEmoji += emoji . Mark ;
					}
				}

				stopwatch . Stop ( ) ;


				if ( e . Message . Chat . Type == ChatType . Private )
				{
					BotClient . SendTextMessageAsync ( message . Chat . Id ,
														$"TextLenth:{lenth}" + Environment . NewLine +
														$"EmojiLenth:{emojiLenth}" + Environment . NewLine +
														$"EmojiCount:{amountOfEmoji}" + Environment . NewLine +
														$"EmojiKind:{usedEmojiTypeCount}" + Environment . NewLine +
														$"Cost:{stopwatch . Elapsed}" ,
														replyToMessageId : message . MessageId ) ;
				}
				else if ( e . Message . Chat . Type == ChatType . Group ||
						e . Message . Chat . Type == ChatType . Supergroup )
				{
					if ( emojiLenth * 1.1 >= lenth &&
						usedEmojiTypeCount >= 2 )
					{
						BotClient . DeleteMessageAsync ( message . Chat . Id , message . MessageId ) ;
					}
				}
			}
		}


		public override void ShowLogo ( )
		{
			Console . WriteLine ( new AsciiArt ( "AntiPlaneWuBot" , width : CharacterWidth . Smush ) ) ;
		}

		public override void ShowCopyright ( )
		{
			Console . WriteLine ( $"AntiPlaneWuBot Copyright (C) 2018 - {DateTime . Now . Year} Wencey Wang" ) ;
			Console . WriteLine ( @"This program comes with ABSOLUTELY NO WARRANTY." ) ;
			Console . WriteLine (
				@"This is free software, and you are welcome to redistribute it under certain conditions; read License.txt for details." ) ;
		}

		public override void OnExit ( ProgramExitCode code ) { BotClient ? . StopReceiving ( ) ; }

		public static void Main ( string [ ] args )
		{
#if DEBUG
			StaticLoggerFactory . LoggerFactory =
				new LoggerFactory ( ) . AddConsole ( LogLevel . Trace ) . AddDebug ( ) ;
#else
			StaticLoggerFactory . LoggerFactory =
				new LoggerFactory ( ) . AddConsole ( LogLevel . Information ) . AddDebug ( ) ;
#endif

			new Program ( ) . RunMain ( args ) ;
		}

	}

}
