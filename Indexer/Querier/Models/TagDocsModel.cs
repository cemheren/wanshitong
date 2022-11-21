
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class TagDocModel
{

    [Required]
    public Dictionary<string, string> IndexAndDocId;

    [Required]
    public string Tag;
}