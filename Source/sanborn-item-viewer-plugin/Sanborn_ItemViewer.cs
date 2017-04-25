#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using SobekCM.Core.BriefItem;
using SobekCM.Core.FileSystems;
using SobekCM.Core.Navigation;
using SobekCM.Core.Users;
using SobekCM.Library.ItemViewer.Menu;
using SobekCM.Tools;
using System.Text;
using SobekCM.Library.ItemViewer.Viewers;
#endregion

namespace SanbornViewer
{
    public class Sanborn_ItemViewer_Prototyper : abstractItemViewerPrototyper
    {
        /// <summary> Constructor for a new instance of the Sanborn_ItemViewer_Prototyper class </summary>
        public Sanborn_ItemViewer_Prototyper()
        {
            ViewerType = "SANBORN";
            ViewerCode = "sanborn";
        }

        /// <summary> Indicates if the specified item matches the basic requirements for this viewer, or
        /// if this viewer should be ignored for this item </summary>
        /// <param name="CurrentItem"> Digital resource to examine to see if this viewer really should be included </param>
        /// <returns> TRUE if this viewer should generally be included with this item, otherwise FALSE </returns>
        public override bool Include_Viewer(BriefItemInfo CurrentItem)
        {
            // Have to actually look in the digital resource folder
            return SobekFileSystem.FileExists(CurrentItem, CurrentItem.BibID + "_" + CurrentItem.VID + ".gif");
        }

        /// <summary> Flag indicates if this viewer should be override on checkout </summary>
        /// <param name="CurrentItem"> Digital resource to examine to see if this viewer should really be overriden </param>
        /// <returns> TRUE always, since PDFs should never be shown if an item is checked out </returns>
        public override bool Override_On_Checkout(BriefItemInfo CurrentItem)
        {
            return true;
        }

        /// <summary> Flag indicates if the current user has access to this viewer for the item </summary>
        /// <param name="CurrentItem"> Digital resource to see if the current user has correct permissions to use this viewer </param>
        /// <param name="CurrentUser"> Current user, who may or may not be logged on </param>
        /// <param name="IpRestricted"> Flag indicates if this item is IP restricted AND if the current user is outside the ranges </param>
        /// <returns> TRUE if the user has access to use this viewer, otherwise FALSE </returns>
        public override bool Has_Access(BriefItemInfo CurrentItem, User_Object CurrentUser, bool IpRestricted)
        {
            return !IpRestricted;
        }

        /// <summary> Gets the menu items related to this viewer that should be included on the main item (digital resource) menu </summary>
        /// <param name="CurrentItem"> Digital resource object, which can be used to ensure if and how this viewer should appear 
        /// in the main item (digital resource) menu </param>
        /// <param name="CurrentUser"> Current user, who may or may not be logged on </param>
        /// <param name="CurrentRequest"> Information about the current request </param>
        /// <param name="MenuItems"> List of menu items, to which this method may add one or more menu items </param>
        /// <param name="IpRestricted"> Flag indicates if this item is IP restricted AND if the current user is outside the ranges </param>
        public override void Add_Menu_Items(BriefItemInfo CurrentItem, User_Object CurrentUser, Navigation_Object CurrentRequest, List<Item_MenuItem> MenuItems, bool IpRestricted)
        {
            // Get the URL for this
            string previous_code = CurrentRequest.ViewerCode;
            CurrentRequest.ViewerCode = ViewerCode.Replace("#", "1");
            string url = UrlWriterHelper.Redirect_URL(CurrentRequest);
            CurrentRequest.ViewerCode = previous_code;

            // Add the item menu information
            Item_MenuItem menuItem = new Item_MenuItem("Index", null, null, url, ViewerCode);
            MenuItems.Add(menuItem);
        }

        /// <summary> Creates and returns the an instance of the <see cref="Sanborn_ItemViewer"/> class for showing a  
        /// JPEG image from a page within a digital resource during execution of a single HTTP request. </summary>
        /// <param name="CurrentItem"> Digital resource object </param>
        /// <param name="CurrentUser"> Current user, who may or may not be logged on </param>
        /// <param name="CurrentRequest"> Information about the current request </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <returns> Fully built and initialized <see cref="JPEG_ItemViewer"/> object </returns>
        /// <remarks> This method is called whenever a request requires the actual viewer to be created to render the HTML for
        /// the digital resource requested.  The created viewer is then destroyed at the end of the request </remarks>
        public override iItemViewer Create_Viewer(BriefItemInfo CurrentItem, User_Object CurrentUser, Navigation_Object CurrentRequest, Custom_Tracer Tracer)
        {
            return new Sanborn_ItemViewer(CurrentItem, CurrentUser, CurrentRequest );
        }
    }


    /// <summary> Item viewer displays the a HTML source file related to this digital resource embedded into the SobekCM window for viewing. </summary>
    /// <remarks> This class extends the abstract class <see cref="abstractNoPaginationItemViewer"/> and implements the 
    /// <see cref="iItemViewer" /> interface. </remarks>
    public class Sanborn_ItemViewer : abstractNoPaginationItemViewer
    {
        private readonly string htmlFile;
        private readonly string gifFile;

        /// <summary> Constructor for a new instance of the Sanborn_ItemViewer class, used to display a HTML file from a digital resource </summary>
        /// <param name="BriefItem"> Digital resource object </param>
        /// <param name="CurrentUser"> Current user, who may or may not be logged on </param>
        /// <param name="CurrentRequest"> Information about the current request </param>
        public Sanborn_ItemViewer(BriefItemInfo BriefItem, User_Object CurrentUser, Navigation_Object CurrentRequest)
        {
            // Save the arguments for use later
            this.BriefItem = BriefItem;
            this.CurrentUser = CurrentUser;
            this.CurrentRequest = CurrentRequest;

            // Set the behavior properties to the empy behaviors ( in the base class )
            Behaviors = EmptyBehaviors;

            // The prototyper checks for existence of the .gif file, so that should exist
            // and we won't both checking again here.
            gifFile = BriefItem.BibID + "_" + BriefItem.VID + ".gif";

            // Check for existence of the HTMl file though
            if ( SobekFileSystem.FileExists(BriefItem, BriefItem.BibID + "_" + BriefItem.VID + ".htm"))
            {
                htmlFile = SobekFileSystem.Resource_Network_Uri(BriefItem, BriefItem.BibID + "_" + BriefItem.VID + ".htm");
            }
        }

        /// <summary> Write the item viewer main section as HTML directly to the HTTP output stream </summary>
        /// <param name="Output"> Response stream for the item viewer to write directly to </param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        public override void Write_Main_Viewer_Section(TextWriter Output, Custom_Tracer Tracer)
        {
            if (Tracer != null)
            {
                Tracer.Add_Trace("Sanborn_ItemViewer.Write_Main_Viewer_Section", "");
            }

            // Save the current viewer code
            string current_view_code = CurrentRequest.ViewerCode;

            // Start the citation table
            Output.WriteLine("\t\t<!-- SANBORN VIEWER OUTPUT -->");
            Output.WriteLine("\t\t<td style=\"align:left;\">");
            Output.WriteLine("\t\t\t<p>Click on a rectangle in the visual index below to view details.</p>");

            // Determine some replacement strings here
            string itemURL = SobekFileSystem.Resource_Web_Uri(BriefItem);
            string itemLink = CurrentRequest.Base_URL + "/" + BriefItem.BibID + "/" + BriefItem.VID;

            // Determine the source string
            string gif_url = SobekFileSystem.Resource_Web_Uri(BriefItem, gifFile);

            // Was there a HTML map for this as well?

            // Look for the HTML map 
            if ( String.IsNullOrWhiteSpace(htmlFile))
            {
                // Add just the GIF image then
                Output.WriteLine("<img alt=\"Sanborn Index\" src=\"" + gif_url + "\" />");
            }
            else
            {
                try
                {
                    string map = SobekFileSystem.ReadToEnd(BriefItem, htmlFile);
                    map = map.Replace("<%ITEM_LINK%>&m=hd", itemLink + "/").Replace("\">", "j\">").Replace("Mapj", "Map");
                    Output.WriteLine(map);
                }
                catch ( Exception ee )
                {

                }

                // Add the image, with a reference to the map
                Output.WriteLine("<img alt=\"Sanborn Index\" src=\"" + gif_url + "\" usemap=\"#Map\" />");

            }

            //// Try to get the HTML for this
            //if (Tracer != null)
            //{
            //    Tracer.Add_Trace("Sanborn_ItemViewer.Write_Main_Viewer_Section", "Reading html for this view from static page");
            //}
            //string map;
            //try
            //{
            //    map = SobekFileSystem.ReadToEnd(BriefItem, sourceString);
            //}
            //catch
            //{
            //    StringBuilder builder = new StringBuilder();
            //    builder.AppendLine("<div style=\"background-color: White; color: black;text-align:center; width:630px;\">");
            //    builder.AppendLine("  <br /><br />");
            //    builder.AppendLine("  <span style=\"font-weight:bold;font-size:1.4em\">Unable to pull html view for item ( <a href=\"" + sourceString + "\">source</a> )</span><br /><br />");
            //    builder.AppendLine("  <span style=\"font-size:1.2em\">We apologize for the inconvenience.</span><br /><br />");

            //    string returnurl = CurrentRequest.Base_URL + "/contact";
            //    builder.AppendLine("  <span style=\"font-size:1.2em\">Click <a href=\"" + returnurl + "\">here</a> to report the problem.</span>");
            //    builder.AppendLine("  <br /><br />");
            //    builder.AppendLine("</div>");
            //    map = builder.ToString();
            //}

            //// Write the HTML 
            //string url_options = UrlWriterHelper.URL_Options(CurrentRequest);
            //string urlOptions1 = String.Empty;
            //string urlOptions2 = String.Empty;
            //if (url_options.Length > 0)
            //{
            //    urlOptions1 = "?" + url_options;
            //    urlOptions2 = "&" + url_options;
            //}
            //Output.WriteLine(map.Replace("<%URLOPTS%>", url_options).Replace("<%?URLOPTS%>", urlOptions1).Replace("<%&URLOPTS%>", urlOptions2).Replace("<%ITEMURL%>", itemURL).Replace("<%ITEM_LINK%>", itemLink));

            // Finish the table
            Output.WriteLine("\t\t</td>");
            Output.WriteLine("\t\t<!-- END SANBORN VIEWER OUTPUT -->");

            // Restore the mode
            CurrentRequest.ViewerCode = current_view_code;
        }

        /// <summary> Allows controls to be added directory to a place holder, rather than just writing to the output HTML stream </summary>
        /// <param name="MainPlaceHolder"> Main place holder ( &quot;mainPlaceHolder&quot; ) in the itemNavForm form into which the bulk of the item viewer's output is displayed</param>
        /// <param name="Tracer"> Trace object keeps a list of each method executed and important milestones in rendering </param>
        /// <remarks> This method does nothing, since nothing is added to the place holder as a control for this item viewer </remarks>
        public override void Add_Main_Viewer_Section(PlaceHolder MainPlaceHolder, Custom_Tracer Tracer)
        {
            // Do nothing
        }
    }
}
