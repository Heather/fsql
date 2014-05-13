#light (*
	exec fsharpi --exec $0 $@
*)

#r "Fakelib.dll"
#r "System.Data.dll"

#I @"src\Design\bin\Release"
#I @"src\FSQL\bin\Release"

#r "WinForms.dll"
#r "FSQL.dll"

open System
open System.IO
open System.Text
open System.Drawing
open System.Windows.Forms

open System.Data
open System.Data.SqlClient

open Fake.StringHelper

open Fsql
open Fsql.Design.WinForms

let datasourceConf = "datasource.conf"
let lastQueryConf  = "lastQery.fsql"

let form = new Form()
form.Width  <- 800
form.Height <- 750
form.Text   <- "Fast SQL"
form.Font   <- new Font( "Lucida Console"
                       , 12.0f
                       , FontStyle.Regular,GraphicsUnit.Point )
                       
form.FormBorderStyle    <- FormBorderStyle.FixedDialog
form.MaximizeBox        <- false
form.MinimizeBox        <- false

let l2 = new Label();
l2.Location <- Point(20,325); l2.Text <- "Output"
let r0 = new RichTextBox();
r0.Location <- Point(20, 25); r0.Size <- Size(form.Width - 50, 40); 

r0.Text <- if File.Exists datasourceConf
                then ReadFileAsString datasourceConf
                else @"Data Source=(LocalDb)\v11.0;Initial Catalog=db;Integrated Security=True"
                
let ddp = new DDP();
ddp.Location <- Point(5, 0); ddp.Size <- Size(form.Width - 20, 70); 

form.Controls.Add r0

let r1 = new RichTextBoxWithSqlHighlighting();
r1.Location <- Point(10, 80); r1.Size <- Size(770, 240); 

r1.Text <- if File.Exists lastQueryConf
                then ReadFileAsString lastQueryConf
                else "! [FirstName], [LastName] ~ [Table]"

let r2 = new RichTextBox();
r2.Location <- Point(10, 350); r2.Size <- Size(770, 300)
let b1 = new Button();
b1.Location <- Point(40, 650); b1.Size <- Size(150, 50); b1.Text <- "Exit"
let b2 = new Button();
b2.Location <- Point(580, 650); b2.Size <- Size(150, 50); b2.Text <- "Go"

let gv = new DataGridView();
gv.Location <- Point(10, 350); gv.Size <- Size(770, 300); gv.Visible <- false

let runQuery () =
    let cmd = fsql r1.Lines
    try
        use conn    = new SqlConnection( r0.Text )
        use command = new SqlCommand(cmd, conn)
        conn.Open()
        if cmd.ToLower().Contains("select")
            then
                let dt = new DataTable()
                use adapter = new SqlDataAdapter(command)
                
                adapter.Fill(dt) |> ignore
                
                WriteToFile false datasourceConf r0.Lines
                WriteToFile false lastQueryConf r1.Lines
                
                r2.Visible <- false
                gv.Visible <- true
                
                gv.DataSource <- dt
                gv.AutoResizeColumns()
            else
                command.ExecuteNonQuery() |> ignore
                r2.Text <- r2.Text + "OK" + Environment.NewLine
        conn.Close()
    with
        | exn -> r2.Visible <- true
                 gv.Visible <- false
                 r2.Text <- r2.Text
                                + "Exception:\n"
                                + exn.Message + Environment.NewLine

b1.Click.Add(fun _ -> ignore <| form.Close())
b2.Click.Add(fun _ -> runQuery())
form.Shown.Add(fun _ -> r1.ColorTheKeyWords())

form.Controls.AddRange [|ddp; l2; b1; b2; r1; r2; gv|]; Application.Run(form)
