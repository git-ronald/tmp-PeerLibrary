using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeerLibrary.PeerApp.Models.GUI;

public class PageColumn
{
    public ICollection<IElement> Elements { get; set; } = new List<IElement>();
}
