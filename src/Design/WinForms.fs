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
    
type FFP() as x =
    inherit System.Windows.Forms.Panel()
    do
        x.SetStyle(ControlStyles.DoubleBuffer, true)
        x.SetStyle(ControlStyles.UserPaint, true)
        x.SetStyle(ControlStyles.AllPaintingInWmPaint, true)

type DDP() as x =
    inherit System.Windows.Forms.Panel()
    let mutable expanded = true
    let mutable roundedCorners = true
    
    let roundedHeaderCollapsed = new GraphicsPath()
    let roundedHeaderExpanded = new GraphicsPath()
    let squaredHeader = new GraphicsPath()
    
    let fullCenteredFormat = new StringFormat()
    let vCenteredFormat = new StringFormat()

    let arrowFont = new Font(FontFamily.GenericMonospace, 16.0f, FontStyle.Bold, GraphicsUnit.Pixel)
    let bmpCollapseNormal   = new Bitmap(12, 12, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
    let bmpCollapseOver     = new Bitmap(12, 12, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
    let mutable bmpExpandNormal : Bitmap = null
    let mutable bmpExpandOver : Bitmap = null
    
    let mutable p : Pen                     = null
    let mutable bmp : Bitmap                = null
    let mutable b : Brush                   = null
    let mutable currIcon : Bitmap           = null
    let mutable currPath : GraphicsPath     = null
    let mutable offset                      = 0.0f
    
    let _pnlHeader = new FFP()
    
    do
        _pnlHeader.Location     <- new Point(0, 0)
        _pnlHeader.Width        <- x.Width
        _pnlHeader.Height       <- 20
        _pnlHeader.BackColor    <- Color.Transparent
        
        x.Controls.Add(x.pnlHeader)
        x.createData()
        x.createBitmaps()
        x.createPaths()
        
        x.pnlHeader.Paint.Add(fun e ->
            let gr = e.Graphics
            gr.SmoothingMode <- SmoothingMode.AntiAlias

            p <- new Pen(SystemBrushes.ControlDark, 1.0f)
            b <- Brushes.Black
            bmp <- if expanded
                    then bmpCollapseNormal
                    else bmpExpandNormal

            currPath <- if roundedCorners
                            then if expanded
                                    then roundedHeaderExpanded
                                    else roundedHeaderCollapsed
                            else squaredHeader

            gr.DrawPath(p, currPath)

            if expanded
                then
                     // Panel is all visible
                     gr.DrawImage(bmp, _pnlHeader.Width - 12 - 6, _pnlHeader.Height / 2 - 7, 12, 12)
                else gr.DrawImage(bmp, _pnlHeader.Width - 12 - 6, _pnlHeader.Height / 2 - 5, 12, 12)

            offset      <- 0.0f
            currIcon    <- null

            if currIcon <> null then
                offset <- (single currIcon.Width) + 4.0f
                gr.DrawImage(currIcon, new Rectangle(6, _pnlHeader.Height / 2 - currIcon.Height / 2, currIcon.Width, currIcon.Height));
            gr.DrawString("FSQL", Control.DefaultFont, b, new RectangleF(6.0f + offset, 0.0f, (single _pnlHeader.Width) - 12.0f - 6.0f - 6.0f, (single _pnlHeader.Height) - 1.0f), vCenteredFormat)
        )
        
        x.SetStyle(ControlStyles.DoubleBuffer, true)
        x.SetStyle(ControlStyles.UserPaint, true)
        x.SetStyle(ControlStyles.AllPaintingInWmPaint, true)

    member x.pnlHeader = _pnlHeader;
    member x.createPaths() =
        roundedHeaderCollapsed.AddLine(5, 0, x.Width - 5 - 1, 0)
        roundedHeaderCollapsed.AddArc(x.Width - 5 - 1 - 5, 5 - 5, 10, 10, -90.0f, 90.0f)
        roundedHeaderCollapsed.AddLine(x.Width - 1, 5, x.Width - 1, x.pnlHeader.Height - 5 - 1)
        roundedHeaderCollapsed.AddArc(x.Width - 5 - 1 - 5, x.pnlHeader.Height - 5 - 1 - 5, 10, 10, 0.0f, 90.0f)
        roundedHeaderCollapsed.AddLine(x.Width - 5 - 1, x.pnlHeader.Height - 1, 5, x.pnlHeader.Height - 1)
        roundedHeaderCollapsed.AddArc(0, x.pnlHeader.Height - 5 - 1 - 5, 10, 10, 90.0f, 90.0f)
        roundedHeaderCollapsed.AddLine(0, x.pnlHeader.Height - 5 - 1, 0, 5)
        roundedHeaderCollapsed.AddArc(0, 0, 10, 10, 180.0f, 90.0f)

        roundedHeaderExpanded.AddLine(5, 0, x.Width - 5 - 1, 0)
        roundedHeaderExpanded.AddArc(x.Width - 5 - 1 - 5, 5 - 5, 10, 10, -90.0f, 90.0f)
        roundedHeaderExpanded.AddLine(x.Width - 1, 5, x.Width - 1, x.pnlHeader.Height - 1)
        roundedHeaderExpanded.AddLine(x.Width - 1, x.pnlHeader.Height - 1, 0, x.pnlHeader.Height - 1)
        roundedHeaderExpanded.AddLine(x.Width - 1, x.pnlHeader.Height - 1, 0, x.pnlHeader.Height - 1)
        roundedHeaderExpanded.AddArc(0, 0, 10, 10, 180.0f, 90.0f)

        squaredHeader.AddLine(0, 0, x.Width - 1, 0)
        squaredHeader.AddLine(x.Width - 1, 0, x.Width - 1, x.pnlHeader.Height - 1)
        squaredHeader.AddLine(x.Width - 1, x.pnlHeader.Height - 1, 0, x.pnlHeader.Height - 1)
        squaredHeader.AddLine(0, x.pnlHeader.Height - 1, 0, 0)
    member x.createData() =
        fullCenteredFormat.Alignment     <- StringAlignment.Center
        fullCenteredFormat.LineAlignment <- StringAlignment.Center
        vCenteredFormat.LineAlignment    <- StringAlignment.Center
    member x.createBitmaps() =
        let s = "»"
        let mutable gr = Graphics.FromImage(bmpCollapseNormal)
        gr.Clear(Color.Transparent)
        gr.DrawString(s, arrowFont, SystemBrushes.ControlDarkDark, new RectangleF(0.5f, 0.5f, 11.0f, 11.0f), fullCenteredFormat)
        gr.Dispose()
        bmpCollapseNormal.RotateFlip(RotateFlipType.Rotate270FlipNone)

        bmpExpandNormal <- bmpCollapseNormal.Clone() :?> Bitmap
        bmpExpandNormal.RotateFlip(RotateFlipType.RotateNoneFlipY)

        gr <- Graphics.FromImage(bmpCollapseOver)
        gr.Clear(Color.Transparent)
        gr.DrawString(s, arrowFont, SystemBrushes.HotTrack, new RectangleF(0.5f, 0.5f, 11.0f, 11.0f), fullCenteredFormat)
        gr.Dispose()
        bmpCollapseOver.RotateFlip(RotateFlipType.Rotate270FlipNone)

        bmpExpandOver <- bmpCollapseOver.Clone() :?> Bitmap
        bmpExpandOver.RotateFlip(RotateFlipType.RotateNoneFlipY)
    override x.OnPaint(e : System.Windows.Forms.PaintEventArgs) =
        if(expanded && x.Height >= 2 * x.pnlHeader.Height) then
            let gr = e.Graphics
            gr.SmoothingMode <- SmoothingMode.AntiAlias
            let p = new Pen(SystemBrushes.ControlDark, 1.0f)
            if roundedCorners
                then
                    gr.DrawLine(p, 0, x.pnlHeader.Height, 0, x.Height - 5 - 1)
                    gr.DrawArc(p, 0, x.Height - 5 - 1 - 5, 10, 10, 90, 90)
                    gr.DrawLine(p, 5, x.Height - 1, x.Width - 5 - 1, x.Height - 1)
                    gr.DrawArc(p, x.Width - 5 - 1 - 5, x.Height - 5 - 1 - 5, 10, 10, 0, 90)
                    gr.DrawLine(p, x.Width - 1, x.Height - 5 - 1, x.Width - 1, x.pnlHeader.Height)
                else
                    gr.DrawLine(p, 0, x.pnlHeader.Height, 0, x.Height)
                    gr.DrawLine(p, 0, x.Height - 1, x.Width - 1, x.Height - 1)
                    gr.DrawLine(p, x.Width - 1, x.Height - 1, x.Width - 1, x.pnlHeader.Height)
        base.OnPaint(e)
    override x.OnSizeChanged(e : System.EventArgs) =
        x.pnlHeader.Width <- x.Width
        x.createPaths(); x.Invalidate(); x.Update()
        base.OnSizeChanged(e)
                
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
        hl "(\*)|(!)|(@)|(let)" Color.Blue
        hl "([)|(])" Color.DarkBlue
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