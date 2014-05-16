module Fsql.Design.WinForms

open System
open System.IO
open System.Text
open System.Drawing
open System.Windows.Forms

open System.Drawing.Drawing2D

open System.Runtime.InteropServices
open System.Text.RegularExpressions

module Lock =
    [<DllImport(@"User32", CharSet = CharSet.Ansi, SetLastError = false, ExactSpelling = true)>]
    extern void LockWindowUpdate(int hWnd)
                
type RichTextBoxWithSqlHighlighting() = 
    inherit System.Windows.Forms.RichTextBox()
    override x.OnTextChanged(e : System.EventArgs) =
        base.OnTextChanged(e); x.ColorTheKeyWords()
    member x.ColorTheKeyWords() =
        let color(m : Match, color : Color) =
            x.SelectionStart    <- m.Index
            x.SelectionLength   <- m.Length
            x.SelectionColor    <- color
        let hl s c =
            Regex.Matches(x.Text, "\\b" + s + "\\b", RegexOptions.IgnoreCase) |> fun mx ->
                for m in mx do if (m.Success) then color(m,c)
        let SelectionAt = x.SelectionStart
        Lock.LockWindowUpdate(x.Handle.ToInt32())
        hl "(\*)|(!)|(~)|(,)|(@)" Color.Blue
        hl "(\()|(\))" Color.DarkGray

        let hlW (w : string) (s : string) c =
            let mutable docolor = false
            for mtch in s.Split('(','|',')') do
                if not docolor && mtch <> "" && w.ToLower() = mtch then 
                    docolor <- true
                    Regex.Matches(x.Text, "\\b" + s + "\\b", RegexOptions.IgnoreCase)
                    |> fun mx -> for m in mx do if (m.Success) then color(m,c)
        for word in x.Text.Split( ' ', '*', '!', '~', ',', '@'
                                , '(', ')', '[', ']'
                                , '\n' ,'\r' ,'\t') do
            hlW word "(let)" Color.Blue
            hlW word "(and)|(or)|(not)" Color.DarkGreen
            hlW word "(avg)|(abs)|(max)|(min)" Color.DarkRed
            hlW word "(select)|(where)|(from)|(top)|(order)|(group)|(by)|(as)|(null)|(insert)|(exec)|(into)" Color.Blue
            hlW word "(desc)|(asc)" Color.Brown
            hlW word "(join)|(left)|(inner)|(outer)|(right)|(on)" Color.Red
            hlW word "(case)|(when)|(then)|(else)|(end)|(if)|(begin)" Color.Teal
            hlW word "(cast)|(nvarchar)|(bit)|(datetime)|(int)|(table)" Color.BlueViolet
            hlW word "(datepart)" Color.DarkOrange

        Regex.Matches(x.Text, "\[(.*?)\]", RegexOptions.IgnoreCase) |> fun mx ->
            for m in mx do if (m.Success) then color(m,Color.DarkBlue)
            
        x.SelectionStart    <- SelectionAt
        x.SelectionLength   <- 0
        x.SelectionColor    <- Color.Black
        Lock.LockWindowUpdate(0)
