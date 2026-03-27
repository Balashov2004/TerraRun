using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TerraRun.Api.Models;

public class Run
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int UserId { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public List<RunPoint> Points { get; set; } = new();
}