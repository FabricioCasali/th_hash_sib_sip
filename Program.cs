using System.Security.Cryptography;
using System.Text;
using System.Xml;

internal class Program
{
    private static void Main(string[] args)
    {
        if (args == null || args.Length == 0)
        {
            Console.WriteLine("informe o caminho para o arquivo de entrada");
            return;
        }

        var path = args[0];
        if (!Path.IsPathFullyQualified(path))
        {
            path = Path.GetFullPath(path);
        }
        Console.WriteLine($"Arquivo de entrada: {path}");
        if (!File.Exists(path))
        {
            Console.WriteLine($"não foi possível ler o arquivo no caminho informado: {path}");
            return;
        }
        var newFile = $"{path}.novo";
        if (args.Length == 2)
        {
            newFile = args[1];
            if (!Path.IsPathFullyQualified(newFile))
            {
                newFile = Path.Combine(Path.GetDirectoryName(path), newFile);
            }
        }
        GenerateHash(path, newFile);

        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Done, agora paga o churras pro ");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write("Casali!");
        Console.ResetColor();
    }

    private static void GenerateHash(string v, string newFile)
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(v);
        xmlDocument.FirstChild.NextSibling.Attributes.RemoveAll();
        var epilogos = xmlDocument.GetElementsByTagName("epilogo");
        epilogos[0].ParentNode.RemoveChild(epilogos[0]);
        var allText = "http://www.ans.gov.br/padroes/sib/schemas http://www.ans.gov.br/padroes/sib/schemas/sib.xsd" + xmlDocument.FirstChild.NextSibling.InnerText;
        using var md5 = MD5.Create();
        using var mss = new MemoryStream(Encoding.GetEncoding("iso8859-1").GetBytes(allText));
        var result = md5.ComputeHash(mss);
        var newEpilogoTag = xmlDocument.CreateElement("epilogo");
        var hashTag = xmlDocument.CreateElement("hash");
        hashTag.InnerText = BitConverter.ToString(result).Replace("-", "").ToUpper();
        newEpilogoTag.AppendChild(hashTag);
        xmlDocument.FirstChild.NextSibling.AppendChild(newEpilogoTag);
        using var xtw = new XmlTextWriter(newFile, Encoding.GetEncoding("ISO8859-1"));
        xtw.Formatting = Formatting.Indented;
        xmlDocument.Save(xtw);
        xtw.Close();
        xtw.Dispose();
        if (newFile.Contains(".sib"))
        {
            var content = File.ReadAllText(newFile);
            content = content.Replace("<mensagemSIB>", @"<mensagemSIB xmlns:ansSIB=""http://www.ans.gov.br/padroes/sib/schemas"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xsi:schemaLocation=""http://www.ans.gov.br/padroes/sib/schemas http://www.ans.gov.br/padroes/sib/schemas/sib.xsd"">");
            File.WriteAllText(newFile, content);
        }
    }
}