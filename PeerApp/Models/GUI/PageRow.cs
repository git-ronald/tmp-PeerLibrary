using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerLibrary.PeerApp.Models.GUI;

public class PageRow
{
    public ICollection<PageColumn> Columns { get; set; } = new List<PageColumn>();
}
