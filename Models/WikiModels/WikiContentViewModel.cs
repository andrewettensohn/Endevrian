using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Endevrian.Models.WikiModels
{
    public class WikiContentViewModel
    {
        public List<WikiPage> SearchResults { get; set; }

        public WikiPage SelectedPage { get; set; }

        public WikiContentViewModel()
        {
            SearchResults = new List<WikiPage>();
            SelectedPage = new WikiPage();
        }
    }
}
