Public Class Form1
#Region "委托定义"
    'textbox：
    Delegate Sub AddShowTextBoxDelegate(textbox As System.Windows.Forms.TextBox, str As String) '//托管程序
    ''' <summary>
    ''' 修改TextBox中内容
    ''' </summary>
    ''' <param name="textbox">TextBox控件的名称</param>
    ''' <param name="str">需要修改的值</param>
    ''' <remarks></remarks>
    Private Sub DG_ChangeTextBoxValue(textbox As System.Windows.Forms.TextBox, str As String) '//托管程序
        If (textbox.InvokeRequired) Then
            Dim d As New AddShowTextBoxDelegate(AddressOf DG_ChangeTextBoxValue)
            textbox.Invoke(d, textbox, str)
        Else
            textbox.Text = str
        End If
    End Sub

    'Label:
    Delegate Sub AddShowLabelDelegate(label As System.Windows.Forms.Label, str As String, Backcolor As Color) '//托管程序
    ''' <summary>
    ''' 修改Label的内容及背景色
    ''' </summary>
    ''' <param name="label">Label控件的名称</param>
    ''' <param name="str">Label的内容</param>
    ''' <param name="Backcolor">需要显示的背景色，默认为WhiteSmoke</param>
    ''' <remarks></remarks>
    Private Sub DG_ChangeLabelValue(label As System.Windows.Forms.Label, str As String, Optional Backcolor As System.Drawing.Color = Nothing) '//托管程序
        If (label.InvokeRequired) Then
            Dim d As New AddShowLabelDelegate(AddressOf DG_ChangeLabelValue) '//可选择为txt sub同样效果
            label.Invoke(d, label, str, Backcolor)
        Else
            label.Text = str
            If Backcolor = Nothing Then
                Backcolor = Color.WhiteSmoke
            End If
            label.BackColor = Backcolor
        End If
    End Sub


#End Region

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        If OpenFileDialog1.ShowDialog() = DialogResult.OK Then
            TextBox1.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        If FolderBrowserDialog1.ShowDialog() = DialogResult.OK Then
            TextBox1.Text = FolderBrowserDialog1.SelectedPath
        End If
    End Sub

    Private Sub Button3_Click(sender As Object, e As EventArgs) Handles Button3.Click
        If FolderBrowserDialog2.ShowDialog() = DialogResult.OK Then
            TextBox2.Text = FolderBrowserDialog2.SelectedPath
        End If
    End Sub

    Public myFloderOperator As FloderOperator = New FloderOperator
    Public finishnumtext As String
    Public allbmpnum, finishnum As Integer

    Private Sub Button4_Click(sender As Object, e As EventArgs) Handles Button4.Click
        If isworking = False Then
            Dim mythread1 As Threading.Thread
            mythread1 = New Threading.Thread(AddressOf ConvertThread)
            mythread1.Start()
        Else
            MessageBox.Show("请等待运行完成")
        End If
    End Sub
    Public isworking As Boolean = False
    Private Sub ConvertThread()
        isworking = True
        bmpfilelist = New List(Of BMPPathAndName)
        bmpfilelist.Clear()
        finishnum = 0
        Try
            If IO.File.Exists(TextBox1.Text) Then
                myFloderOperator.CreatFloder(TextBox2.Text)
                ConvertBMP2JPG(TextBox1.Text, TextBox2.Text + "\" + myFloderOperator.GetFileName(TextBox1.Text).Replace("bmp", "jpg"))
                AddInfo(String.Format("转换{0}成功", TextBox1.Text))
            ElseIf IO.Directory.Exists(TextBox1.Text) Then
                GetBMPList(TextBox1.Text, TextBox1.Text)
                For Each bmpfile In bmpfilelist
                    ConvertBMP2JPG(bmpfile.pathAbs, TextBox2.Text + "\" + bmpfile.PathRelative.Replace("bmp", "jpg"))
                    finishnum += 1
                    DG_ChangeLabelValue(Label3, String.Format("{0}/{1}", finishnum.ToString, allbmpnum.ToString))
                    AddInfo(String.Format("转换{0}成功", bmpfile.pathAbs))
                Next
            Else
                MessageBox.Show("BMP文件输入路径不合法")
            End If
        Catch ex As Exception
            AddInfo("转换过程中发生错误")
            AddInfo(ex.ToString())
            MessageBox.Show("转换过程中发生错误：" + ex.ToString())
        End Try
        isworking = False
    End Sub

    Public bmpfilelist As List(Of BMPPathAndName)
    Public Structure BMPPathAndName
        Dim pathAbs As String
        Dim PathRelative As String
        Dim name As String
    End Structure
    Public Sub GetBMPList(path As String, orgPath As String)
        Dim mDirinfo As System.IO.DirectoryInfo = New IO.DirectoryInfo(path)
        Dim mDirs() As IO.DirectoryInfo = mDirinfo.GetDirectories
        Dim mFiles() As IO.FileInfo = mDirinfo.GetFiles("*.bmp")
        Dim mFiles2() As IO.FileInfo = mDirinfo.GetFiles("*.BMP")
        For Each mFile In mFiles
            Dim PN As BMPPathAndName
            PN.pathAbs = mFile.FullName
            PN.PathRelative = PN.pathAbs.Substring(orgPath.Length, PN.pathAbs.Length - orgPath.Length)
            PN.name = mFile.Name
            bmpfilelist.Add(PN)
        Next
        For Each mFile In mFiles2
            Dim PN As BMPPathAndName
            PN.pathAbs = mFile.FullName
            PN.PathRelative = PN.pathAbs.Substring(orgPath.Length, PN.pathAbs.Length - orgPath.Length)
            PN.name = mFile.Name
            bmpfilelist.Add(PN)
        Next
        For Each mDir In mDirs
            GetBMPList(mDir.FullName, orgPath)
        Next
        allbmpnum = bmpfilelist.Count
    End Sub

    Public Sub ConvertBMP2JPG(BMPpath As String, JPGpath As String)
        Try
            If IO.File.Exists(BMPpath) = True Then
                Dim image As Bitmap = New Bitmap(BMPpath)
                myFloderOperator.CreatFloder(JPGpath)
                image.Save(JPGpath, Imaging.ImageFormat.Jpeg)
            Else

            End If
        Catch ex As Exception
            AddInfo(ex.ToString())
        End Try
    End Sub


#Region "其他维护性工作"
    Delegate Sub AddToInfoList(ByVal txt As String)
    Public ApplicationPath As String = My.Application.Info.DirectoryPath
    Public LogPath As String
    ''' <summary>
    ''' 在界面列表中添加信息
    ''' </summary>
    ''' <param name="str">需要添加的信息</param>
    ''' <remarks></remarks>
    ''' 
    Public Sub AddInfo(ByVal str As String)
        Dim messageinfo As New AddToInfoList(AddressOf AddInfoMain)
        Me.Invoke(messageinfo, str)
    End Sub
    Private Sub AddInfoMain(ByVal str As String)
        str = Now.TimeOfDay.ToString() + " ：" + str
        ListBox_Information.Items.Add(str)
        ListBox_Information.SelectedItem = str
        If ListBox_Information.Items.Count > 3000 Then ListBox_Information.Items.Clear()
        LogPath = ApplicationPath + "\Log\" + Now.Year.ToString() + "-" + Now.Month.ToString() + "-" + Now.Day.ToString() + ".Log"
        If Not IO.File.Exists(LogPath) Then
            'IO.File.Create(LogPath)
            Dim fs As System.IO.FileStream = IO.File.Create(LogPath)
            fs.Close()
        End If
        Dim sw As New System.IO.StreamWriter(LogPath, True)
        sw.WriteLine(str)
        sw.Flush()
        sw.Close()
    End Sub
    Private Sub AddInfoToLog(ByVal str As String)
        Dim sw As New System.IO.StreamWriter(LogPath, True)
        sw.WriteLine(str)
        sw.Flush()
        sw.Close()
    End Sub
#End Region

End Class
Public Class FloderOperator
    ''' <summary>
    ''' 判断此路径是否是文件
    ''' 主要通过最后的文件名中是否存在"."
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function IsFilePath(path As String) As Integer
        Dim mDirNames() As String = path.Split("\")
        If InStr(mDirNames(mDirNames.Length - 1), ".") > 0 Then
            Return mDirNames(mDirNames.Length - 1).Length
        End If
        Return 0
    End Function
    ''' <summary>
    ''' 如果输入的是文件路径，则移除文件，保留文件夹的路径。如果是文件夹路径，则原封不动返回
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function RemoveFilePath(path As String) As String
        If IsFilePath(path) = 0 Then
            Return path
        Else
            Return path.Substring(0, path.Length - IsFilePath(path))
        End If
    End Function
    ''' <summary>
    ''' 返回路径中的文件名
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    Public Function GetFileName(path As String) As String
        Return path.Substring(path.Length - IsFilePath(path), IsFilePath(path))
    End Function
    ''' <summary>
    ''' 创建文件夹
    ''' </summary>
    ''' <param name="path"></param>
    Public Sub CreatFloder(path As String)
        Dim dirpath As String = RemoveFilePath(path)
        If IO.Directory.Exists(IO.Directory.GetParent(dirpath).FullName) = True Then
            IO.Directory.CreateDirectory(dirpath)
        Else
            CreatFloder(IO.Directory.GetParent(dirpath).FullName)
            IO.Directory.CreateDirectory(dirpath)
        End If
    End Sub
End Class
