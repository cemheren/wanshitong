using System.Text;

public class OcrResponse
{
    public string Language { get; set; }

    public Region[] Regions { get; set; }


    public string GetString()
    {
        var sb = new StringBuilder();

        if (Regions == null)
        {
            return "OCR failed";
        }

        foreach (var region in Regions)
        {
            foreach (var line in region.Lines)
            {
                foreach (var word in line.Words)
                {
                    sb.Append(word.Text);
                    sb.Append(" ");
                }

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}

public class Region
{
    public string BoundingBox { get; set; }

    public Line[] Lines { get; set; }
}

public class Line
{
    public string BoundingBox { get; set; }

    public Word[] Words { get; set; }
}

public class Word
{
    public string BoundingBox { get; set; }

    public string Text { get; set; } 
}