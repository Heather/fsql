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
        let hl s c =
            let color(m : Match, color : Color) =
                x.SelectionStart    <- m.Index
                x.SelectionLength   <- m.Length
                x.SelectionColor    <- color
            Regex.Matches(x.Text, "\\b" + s + "\\b", RegexOptions.IgnoreCase) |> fun mx ->
                for m in mx do if (m.Success) then color(m,c)
        let SelectionAt = x.SelectionStart
        Lock.LockWindowUpdate(x.Handle.ToInt32())
        hl "(\*)|(!)|(~)|(,)|(@)|(let)" Color.Blue
        hl "(\()|(\))|(\[)|(\])" Color.DarkGray
        hl "(and)|(or)|(not)" Color.DarkGreen
        hl "(avg)|(abs)|(max)|(min)" Color.DarkRed
        hl "(select)|(where)|(from)|(top)|(order)|(group)|(by)|(as)|(null)|(insert)|(exec)|(into)" Color.Blue
        hl "(join)|(left)|(inner)|(outer)|(right)|(on)" Color.Red
        hl "(case)|(when)|(then)|(else)|(end)|(if)|(begin)" Color.Teal
        hl "(cast)|(nvarchar)|(bit)|(datetime)|(int)|(table)" Color.BlueViolet
        hl "(datepart)" Color.DarkOrange
        x.SelectionStart    <- SelectionAt
        x.SelectionLength   <- 0
        x.SelectionColor    <- Color.Black
        Lock.LockWindowUpdate(0)
