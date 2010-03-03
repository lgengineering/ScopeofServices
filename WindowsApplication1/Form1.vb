Public Class Form1
    '100
    'A globally unique Identifier.  Each Attachment_B document gets one
    Dim GUID As String
    Dim htFCtoSVC As Hashtable
    Dim htFCtoFLAG As Hashtable
    Dim conn As New SqlClient.SqlConnection("Data Source=missas01;Initial Catalog=GeneralizedScope;Persist Security Info=True;User ID=sa;Password=sa")

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim doc_id As Integer
        Dim fc_id As Integer

        doc_id = 4

        htFCtoSVC = New Hashtable
        htFCtoSVC.Add(1, dgFC110Svc)
        htFCtoSVC.Add(2, dgFC120Svc)
        htFCtoSVC.Add(3, dgFC130Svc)
        htFCtoSVC.Add(4, dgFC150Svc)
        htFCtoSVC.Add(5, dgFC160Svc)
        htFCtoSVC.Add(6, dgFC161Svc)
        htFCtoSVC.Add(7, dgFC162Svc)
        htFCtoSVC.Add(8, dgFC163Svc)
        htFCtoSVC.Add(9, dgFC170Svc)
        htFCtoFLAG = New Hashtable
        htFCtoFLAG.Add(1, dgFC110Flags)
        htFCtoFLAG.Add(2, dgFC120Flags)
        htFCtoFLAG.Add(3, dgFC130Flags)
        htFCtoFLAG.Add(4, dgFC150Flags)
        htFCtoFLAG.Add(5, dgFC160Flags)
        htFCtoFLAG.Add(6, dgFC161Flags)
        htFCtoFLAG.Add(7, dgFC162Flags)
        htFCtoFLAG.Add(8, dgFC163Flags)
        htFCtoFLAG.Add(9, dgFC170Flags)
        'Parent id= the row_id field of the NODE table.  Specifies parent function code listed in NODE
        Dim init_parent_id = 0
        For fc_id = 1 To 9
            PopulateDataView(init_parent_id, fc_id)
            PopulateFlagsView(fc_id, doc_id)
        Next
    End Sub

    Private Sub PopulateFlagsView(ByVal inFC As Integer, ByVal inDocID As Integer)
        Dim dgTemp As DataGridView
        Dim check As Integer
        Dim drFlags As SqlClient.SqlDataReader
        Dim tblParent As New DataTable
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        cmdSqlCmd.CommandType = CommandType.StoredProcedure
        cmdSqlCmd.CommandText = "spGetFlags"
        cmdSqlCmd.Parameters.Add("@FCID", SqlDbType.Int)
        cmdSqlCmd.Parameters("@FCID").Value = inFC
        cmdSqlCmd.Parameters.Add("@DocID", SqlDbType.Int)
        cmdSqlCmd.Parameters("@DocID").Value = inDocID
        cmdSqlCmd.Connection = conn
        conn.Open()
        drFlags = cmdSqlCmd.ExecuteReader()
        tblParent.Load(drFlags)
        drFlags.Close()
        conn.Close()
        For Each ParentRow In tblParent.Rows
            Dim dgvRow As New DataGridViewRow
            Dim dgvCell As DataGridViewCell
            If (ParentRow.Item("is_task")) Then
                dgvCell = New DataGridViewCheckBoxCell()
                dgvCell.Value = ParentRow.Item("engineer_flag")
                dgvRow.Cells.Add(dgvCell)

                dgvCell = New DataGridViewCheckBoxCell()
                dgvCell.Value = ParentRow.Item("company_flag")
                dgvRow.Cells.Add(dgvCell)
            Else
                'enter a blank row
                dgvCell = New DataGridViewTextBoxCell
                dgvCell.Value = " "
                dgvRow.Cells.Add(dgvCell)
                dgvCell = New DataGridViewTextBoxCell
                dgvCell.Value = " "
                dgvRow.Cells.Add(dgvCell)
            End If
            dgvRow.Tag = ParentRow.Item("row_id")
            dgTemp = htFCtoFLAG(inFC)
            dgTemp.Rows.Add(dgvRow)
        Next
    End Sub

    Private Sub PopulateDataView(ByVal inParentID As Integer, ByVal inFC As Integer)
        Dim check As Integer
        Dim dgTemp As DataGridView
        Dim drServices As SqlClient.SqlDataReader
        Dim tblParent As New DataTable
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        cmdSqlCmd.CommandType = CommandType.StoredProcedure
        cmdSqlCmd.CommandText = "spGetServices"
        cmdSqlCmd.Parameters.Add("@FCID", SqlDbType.Int)
        cmdSqlCmd.Parameters("@FCID").Value = inFC
        cmdSqlCmd.Parameters.Add("@ParentID", SqlDbType.Int)
        cmdSqlCmd.Parameters("@ParentID").Value = inParentID
        cmdSqlCmd.Connection = conn
        conn.Open()
        drServices = cmdSqlCmd.ExecuteReader()
        tblParent.Load(drServices)
        drServices.Close()
        conn.Close()

        Dim ParentRow As DataRow
        For Each ParentRow In tblParent.Rows
            Dim strLabel As String = ParentRow.Item("label") & " " & ParentRow.Item("node_name")
            Dim dgvRow As New DataGridViewRow
            Dim dgvCell As DataGridViewCell
            dgvCell = New DataGridViewTextBoxCell()
            With dgvCell.Style
                .Padding = New Padding(15 * CInt(ParentRow.Item("indent")), _
                .Padding.Top, _
                .Padding.Right, _
                .Padding.Bottom)
            End With
            dgvCell.Value = strLabel
            dgvRow.Cells.Add(dgvCell)
            dgTemp = htFCtoSVC(inFC)
            dgTemp.Rows.Add(dgvRow)
            ''Insert new row into ATT_B_DATA table
            'conn.Open()
            'cmdSqlCmd = conn.CreateCommand
            'check = cmdSqlCmd.ExecuteReader.RecordsAffected()
            'If check <= 0 Then
            '    MsgBox("Error adding row to ATT_B_DATA")
            'End If
            'conn.Close()
            PopulateDataView(ParentRow.Item("node_id"), inFC)
        Next
    End Sub

    Private Sub TabControl1_Selected(ByVal sender As Object, ByVal e As TabControlEventArgs) _
         Handles TabControl1.Selected
        Dim messageBoxVB As New System.Text.StringBuilder()
        messageBoxVB.AppendFormat("{0} = {1}", "TabPage", e.TabPage)
        messageBoxVB.AppendLine()
        messageBoxVB.AppendFormat("{0} = {1}", "TabPageIndex", e.TabPageIndex)
        messageBoxVB.AppendLine()
        messageBoxVB.AppendFormat("{0} = {1}", "Action", e.Action)
        messageBoxVB.AppendLine()
        ' MessageBox.Show(messageBoxVB.ToString(), "Selected Event")

    End Sub

    Private Sub dgFC170Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC170Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC170Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC170Flags.CurrentRow.Cells(1)
            If (dgFC170Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC170Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC170Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgFC110Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC110Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC110Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC110Flags.CurrentRow.Cells(1)
            If (dgFC110Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC110Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC110Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgFC120Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC120Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC120Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC120Flags.CurrentRow.Cells(1)
            If (dgFC120Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC120Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC120Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgFC130Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC130Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC130Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC130Flags.CurrentRow.Cells(1)
            If (dgFC130Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC130Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC130Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgFC150Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC150Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC150Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC150Flags.CurrentRow.Cells(1)
            If (dgFC150Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC150Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC150Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgFC160Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC160Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC160Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC160Flags.CurrentRow.Cells(1)
            If (dgFC160Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC160Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC160Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgFC161Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC161Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC161Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC161Flags.CurrentRow.Cells(1)
            If (dgFC161Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC161Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC161Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgFC162Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC162Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC162Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC162Flags.CurrentRow.Cells(1)
            If (dgFC162Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC162Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC162Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub

    Private Sub dgFC163Flags_CurrentCellDirtyStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles dgFC163Flags.CurrentCellDirtyStateChanged
        Dim check As Integer
        Dim dgCheckBoxCell1 As DataGridViewCheckBoxCell
        Dim dgCheckBoxCell2 As DataGridViewCheckBoxCell
        Dim cmdSqlCmd As New SqlClient.SqlCommand
        Try
            'For the current row, get the values of the checkboxes in each column
            dgCheckBoxCell1 = dgFC163Flags.CurrentRow.Cells(0)
            dgCheckBoxCell2 = dgFC163Flags.CurrentRow.Cells(1)
            If (dgFC163Flags.IsCurrentCellDirty) Then 'Checking for dirty cell
                dgFC163Flags.CommitEdit(DataGridViewDataErrorContexts.Commit) 'If it is dirty, making them to commit
                conn.Open()
                cmdSqlCmd = conn.CreateCommand
                cmdSqlCmd.CommandText = "UPDATE ATT_B_DATA SET engineer_flag=" & CInt(dgCheckBoxCell1.Value) & _
                ", company_flag=" & CInt(dgCheckBoxCell2.Value) & " WHERE row_id=" & dgFC163Flags.CurrentRow.Tag
                check = cmdSqlCmd.ExecuteReader.RecordsAffected()
                If check <= 0 Then
                    MsgBox("Error adding row to ATT_B_DATA")
                End If
                conn.Close()
            End If
        Catch ex As Exception
        End Try
    End Sub
    Private Sub cbfc170alle_checkstatechanged(ByVal sender As System.Object, _
       ByVal e As System.EventArgs) Handles cbFC170AllE.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC170AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC170Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC170AllE.Checked = False Then
            For Each dgvr In dgFC170Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub


    Private Sub cbFC170AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC170AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow


        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC170AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC170Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC170AllC.Checked = False Then
            For Each dgvr In dgFC170Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If
    End Sub

    Private Sub cbFC110AllE_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC110AllE.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC110AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC110Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC110AllE.Checked = False Then
            For Each dgvr In dgFC110Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub

    Private Sub cbFC110AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC110AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC110AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC110Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC110AllC.Checked = False Then
            For Each dgvr In dgFC110Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If

    End Sub


    Private Sub cbFC120AllE_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC120AllE.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC120AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC120Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC120AllE.Checked = False Then
            For Each dgvr In dgFC120Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC120AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC120AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC120AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC120Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC120AllC.Checked = False Then
            For Each dgvr In dgFC120Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC130AllE_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC130AllE.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC130AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC130Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC130AllE.Checked = False Then
            For Each dgvr In dgFC130Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC130AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC130AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC130AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC130Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC130AllC.Checked = False Then
            For Each dgvr In dgFC130Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC150AllE_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC150AllE.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC150AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC150Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC150AllE.Checked = False Then
            For Each dgvr In dgFC150Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC150AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC150AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC150AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC150Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC150AllC.Checked = False Then
            For Each dgvr In dgFC150Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC160AllE_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC160AllE.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC160AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC160Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC160AllE.Checked = False Then
            For Each dgvr In dgFC160Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC160AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC160AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC160AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC160Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC160AllC.Checked = False Then
            For Each dgvr In dgFC160Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC161AllE_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC161AllE.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC161AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC161Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC161AllE.Checked = False Then
            For Each dgvr In dgFC161Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC161AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC161AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC161AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC161Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC161AllC.Checked = False Then
            For Each dgvr In dgFC161Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC162AllE_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC162AllE.CheckStateChanged

        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC162AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC162Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC162AllE.Checked = False Then
            For Each dgvr In dgFC162Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC162AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC162AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC162AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC162Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC162AllC.Checked = False Then
            For Each dgvr In dgFC162Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC163AllE_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC163AllE.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC163AllE.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC163Flags.Rows
                dgvr.Cells(0).Value = True
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        ElseIf cbFC163AllE.Checked = False Then
            For Each dgvr In dgFC163Flags.Rows
                dgvr.Cells(0).Value = False
                If TypeOf dgvr.Cells(0) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(0).Value = ""
                End If
            Next
        End If

    End Sub
    Private Sub cbFC163AllC_CheckStateChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cbFC163AllC.CheckStateChanged
        Dim dgvr As DataGridViewRow

        'Check main checkbox value to select all or unselect all checkboxes
        If cbFC163AllC.Checked = True Then
            'Loop through each cell and check each checkbox and give it main checkbox value
            For Each dgvr In dgFC163Flags.Rows
                dgvr.Cells(1).Value = True
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        ElseIf cbFC163AllC.Checked = False Then
            For Each dgvr In dgFC163Flags.Rows
                dgvr.Cells(1).Value = False
                If TypeOf dgvr.Cells(1) Is DataGridViewTextBoxCell Then
                    dgvr.Cells(1).Value = ""
                End If
            Next
        End If

    End Sub
End Class

