using System;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace XnTools {

	[CreateAssetMenu( fileName = "---ReadMe---", menuName = "ScriptableObjects/ReadMe", order = 0 )]
	public class XnReadMe_SO : ScriptableObject {
		public Texture2D icon = null;
		// Assets/___Scripts/XnTools/ProjectInfo Icons/ReadMe.png

		[HideInInspector]
		public float iconMaxWidth = 128f;

		[TextArea( 1, 10 )]
		public string projectName = "Project Name";
		public string author           = "Author Name";
		public string modificationDate = currentDate;

		public Section[] sections = readMeDefaultSections;

		[HideInInspector]
		public bool showReadMeEditor = false;

		public void ResetReadMeToDefaults() {
			projectName = "Project Name";
			author = "Author Name";
			sections = readMeDefaultSections;
		}

		public string ToMarkDownString() {
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			sb.AppendLine( $"# **{projectName.Replace( '\n', ' ' )}** - ReadMe File\n" );
			sb.AppendLine( $"#### Author: *{author}*\n" );
			sb.AppendLine( $"##### Modified: *{modificationDate}*\n" );
			sb.AppendLine( "\n<br>\n" );
			foreach ( Section sec in sections ) {
				sb.AppendLine( sec.ToMarkDownString() );
			}

			return sb.ToString();
		}

		static public string currentDate {
			get {
				// Update the date
				DateTime now = DateTime.Now;
				return $"{now.Year:0000}-{now.Month:00}-{now.Day:00}";
			}
		}

		static private Section[] readMeDefaultSections {
			get {
				List<Section> sections = new List<Section>();
				Section sec;
				
				// Itch.io Link
				sec = new Section();
				sec.heading = "0. Default Section Header";
				sec.text = "Section Text Here";
				sections.Add( sec );

				return sections.ToArray();
			}
		}


		[Serializable]
		public class Section {
			[TextArea( 1, 10 )]
			public string heading, text; //, linkText, url;

			static public Section demo {
				get {
					Section sec = new Section();
					sec.heading = "Section Heading";
					sec.text =
						"<b>Section</b> <i>text</i>.\nHold <b>option/alt</b> and click this to Show Default Inspector.";
					return sec;
				}
			}

			public string ToMarkDownString() {
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.AppendLine(
					$"**{heading}**   " ); // Note: The extra spaces at the end of the line tell MarkDown to add <br>
				sb.AppendLine( "\n> &nbsp;" );
				if ( String.IsNullOrEmpty( text ) ) {
					sb.AppendLine( $">*No answer given.*" );
					// if ( url == "" ) {
					// 	sb.AppendLine( $">*No answer given.*" );
					// }
				} else {
					if ( sb == null || text == null ) {
						Debug.Log( "BREAK" );
					} else {
						sb.AppendLine( $">{text.Replace( "\n", "    \n> " )}   " );
					}
				}

				// Actually, this whole link extraction isn't needed in GitLab or GitHub
				// because they extract URLs automatically! - JGB 2024-08-28
				/*
				// Extract the URLs in the text and make them individual buttons
				List<string> urlList = ExtractUrls( text );
				if ( urlList.Count > 0 ) {
					sb.AppendLine( $"   \n>   \n> ***Links in this section***   " );
				}
				foreach (string url in urlList) {
					sb.AppendLine( $">[{url}]({url})    " );
				}

				// if ( url != "" ) {
				// 	if ( linkText != "" ) {
				// 		sb.AppendLine( $">[{linkText}]({url})" );
				// 	} else {
				// 		sb.AppendLine( $"><{url}>" );
				// 	}
				// }
				*/
				sb.AppendLine( "> &nbsp;\n \n" );
				return sb.ToString();
			}

			public List<string> ExtractUrls( string str ) {
				var urls = new List<string>();
				if ( string.IsNullOrEmpty( str ) ) {
					return urls;
				}

				var regex = new Regex( @"https?://[^\s]+", RegexOptions.Compiled | RegexOptions.IgnoreCase );
				var matches = regex.Matches( str );

				foreach ( Match match in matches ) {
					urls.Add( match.Value );
				}

				return urls;
			}
		}


	}
}