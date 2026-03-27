using System.ComponentModel.DataAnnotations;

namespace TerraRun.Api.Models;

public class CapturedCell
{
    [Key]
    public string H3Index { get; set; }
    public int OwnerUserId { get; set; }
    public DateTime CapturedAt { get; set; }
}