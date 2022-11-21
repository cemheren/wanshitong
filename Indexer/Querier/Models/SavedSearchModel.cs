
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public class SavedSearchModel
{

    [Required]
    public string SearchPhrase {get; set;}

    public override bool Equals(object other)
    {
        return this.SearchPhrase.Equals(((SavedSearchModel)other).SearchPhrase, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return this.SearchPhrase.GetHashCode();
    }
}