using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;


namespace BE.Models;

[Table("tasks_table")]
public class Tasque
{
    [Key]
    public int id { get; set; }
    public int ownerid {get;set;}
    [Column(TypeName = "varchar")]
    [StringLength(128)]
    public string title {get; set;}
    [Column(TypeName = "varchar")]
    [StringLength(1024)]
    public string description {get; set;}
    //хранение в качестве json
    //среди деталей: кому назначено, и  статус выполнения
    [Column(TypeName = "jsonb")]
    public TasqueStatus? status {get; set;}
}
[Table("server_properties")]
public class serverProps
{
    [Key]
    public int id {get; set;}
    public int totaltasks {get; set;}
    public int completedtasks {get; set;}
}
public class TasqueStatus   
{
    [JsonPropertyName("assignedUsers")]
    public List<int> assignedUsers {get;set;} = new();
    [JsonPropertyName("isClosed")]
    public bool isClosed {get;set;}
}
public class TaskReportDto
{
    public int idx {get;set;}
}