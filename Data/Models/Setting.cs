using System.ComponentModel.DataAnnotations;

namespace PeerLibrary.Data.Models;

public class Setting
{
    [Key]
    [MaxLength(50)]
    public string Key { get; set; } = String.Empty;
    public string Value { get; set; } = String.Empty;
}
