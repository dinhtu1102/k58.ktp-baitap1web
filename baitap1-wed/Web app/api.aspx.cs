// api.aspx.cs
using System;
using System.Text;
using TuPuzzleLib;

public partial class api : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Response.ContentType = "application/json";
        string action = Request.Form["action"] ?? Request.QueryString["action"] ?? "solve";
        SlidingPuzzle sp = new SlidingPuzzle(4, 4);
        if (action == "shuffle")
        {
            string sig = Request.Form["signature"] ?? "Tu";
            sp.InitGoal();
            sp.SignatureShuffle(sig);
            int[] b = sp.Board;
            string json = "{ \"solvable\": " + (sp.IsSolvable() ? "true" : "false") +
                ", \"board\": [" + string.Join(",", Array.ConvertAll(b, new Converter<int, string>(delegate (int x) { return x.ToString(); }))) + "]" +
                ", \"author\": \"" + sp.AuthorSignature + "\" }";
            Response.Write(json);
            return;
        }
        // default: try to solve board posted by client
        string boardStr = Request.Form["board"] ?? "";
        if (boardStr.Length > 0)
        {
            string[] parts = boardStr.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            int[] arr = new int[parts.Length];
            for (int i = 0; i < parts.Length; i++) arr[i] = int.Parse(parts[i]);
            sp.Board = arr;
        }
        else
        {
            sp.Shuffle(40);
        }

        int[] sol;
        bool ok = sp.Solve(out sol, 80);
        int[] curBoard = sp.Board;
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append("\"solvable\":").Append(ok ? "true" : "false").Append(",");
        sb.Append("\"steps\":").Append(ok ? sol.Length : 0).Append(",");
        sb.Append("\"moves\":[" + (ok ? string.Join(",", Array.ConvertAll(sol, new Converter<int, string>(delegate (int x) { return x.ToString(); }))) : "") + "],");
        sb.Append("\"board\":[" + string.Join(",", Array.ConvertAll(curBoard, new Converter<int, string>(delegate (int x) { return x.ToString(); }))) + "],");
        sb.Append("\"author\":\"" + sp.AuthorSignature + "\"");
        sb.Append("}");
        Response.Write(sb.ToString());
    }
}
