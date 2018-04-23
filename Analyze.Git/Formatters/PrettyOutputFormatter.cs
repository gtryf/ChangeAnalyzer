namespace Analyze.Formatters
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Collections.Generic;
    using ChangeAnalysis.Models;
    using System.Net;

    public class PrettyOutputFormatter : IOutputFormatter
    {
        private StreamWriter File { get; }

        public PrettyOutputFormatter(string fileName)
        {
            this.File = System.IO.File.CreateText(fileName);
        }

        public void WriteHeading(string solutionPath, string commmitId)
        {
            File.WriteLine("<html><head><title>Change Analysis Report</title><style>body{font-family:sans-serif;background:#F2F5F5;padding:20px;color:#5d5d5d}span.toggle-methodlist::after,span.toggle::after{content:\"(toggle)\"}span.toggle,span.toggle-methodlist{font-size:13px;font-family:sans-serif;font-style:italic;}.addition{color:#006400}.deletion{color:red}h1,h2{font-weight:400;padding:5px 10px;color:#fff;margin-bottom:0}.method-symbol,.project{font-family:monospace}h1{background:#555;font-size:24px}h2{font-size:18px;background:#744AF9}.pretxt{padding:10px;margin:0 0 30px;font-size:14px;background:#fff;border-bottom:1px solid #744AF9}.pretxt p:last-child{margin-bottom:0}ul.asset-list,ul.method-list,ul.project-list{margin-top:0;background:#fff;padding:10px 10px 10px 30px;border-bottom:1px solid #744AF9}</style></head><body><h1>Change Analysis Report</h1>");
            File.WriteLine($"<div class='pretxt'><p>Solution path: <i>{WebUtility.HtmlEncode(solutionPath)}</i></p>");
            File.WriteLine($"<p>Commit ID: <i>{WebUtility.HtmlEncode(commmitId)}</i>");
            File.WriteLine($"<p>Date created: <i>{DateTime.Now}</i></p></p>");
        }

        public void WriteChangedAssets(string assetType, IEnumerable<Change> assets)
        {
            if (assets.Any())
            {
                var title = $"Changed {assetType}:";
                File.WriteLine($"<h2>{title}<span class='toggle'></span></h2><ul class='asset-list'>");
                foreach (var asset in assets)
                {
                    File.WriteLine($"<li class='changed-asset {asset.ChangeType.ToString().ToLower()}'>{asset}</li>");
                }
                File.WriteLine("</ul>");
            }
        }

        public void WriteChangedMethods(IEnumerable<ChangedMethod> changes)
        {
            var title = "Methods that have changed:";
            File.WriteLine($"<h2>{title}<span class='toggle'></span></h2><ul class='method-list'>");
            foreach (var method in changes)
            {
                File.WriteLine($"<li class='method-symbol'>{WebUtility.HtmlEncode(method.Symbol)}<ul class='location-list'>");
                foreach (var location in method.Locations)
                    File.Write($"<li class='symbol-location'>{WebUtility.HtmlEncode(location)}</li>");
                File.WriteLine("</ul></li>");
            }
            File.WriteLine("</ul>");
        }

        public void WriteReferencingProjects(IDictionary<string, IList<string>> projects)
        {
            var title = $"Projects that need to be recompiled:";
            File.WriteLine($"<h2>{title}<span class='toggle'></span></h2><ul class='project-list'>");
            foreach (var project in projects)
            {
                File.WriteLine($"<li class='project'>{WebUtility.HtmlEncode(project.Key)} <span class='toggle-methodlist'></span><ul class='referenced-methods' style='display:none;'>");
                foreach (var method in project.Value)
                    File.WriteLine($"<li class='referenced-method'>References method: {WebUtility.HtmlEncode(method)}</li>");
                File.WriteLine("</ul></li>");
            }
            File.WriteLine("</ul>");
        }

        public void AppendSectionBreak()
        {
            File.WriteLine("<br/>");
        }

        public void WriteFooter()
        {
            File.WriteLine("<script src=\"http://code.jquery.com/jquery-2.2.4.min.js\" integrity=\"sha256-BbhdlvQf/xTY9gja0Dq3HiwQF8LaCRTXxZKRutelT44=\" crossorigin=\"anonymous\"></script>");
            File.WriteLine("<script>$(function(){$('.toggle').on('click',function(){$(this).parent().next('ul').toggle();});$('.toggle-methodlist').on('click',function(){$(this).next('ul').toggle();});});</script>");
            File.WriteLine("</body></html>");
            File.Flush();
            File.Close();
        }
    }
}
