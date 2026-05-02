using System.Windows.Forms;

namespace InvoiceBuilder
{
    partial class InvoiceBuilder
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            TableLayoutMain = new TableLayoutPanel();
            tableLayoutHeader = new TableLayoutPanel();
            lblInvoiceNumHeader = new Label();
            lblDateHeader = new Label();
            lblVesselHeader = new Label();
            lblInvoiceNum = new Label();
            txtVessel = new TextBox();
            dtpDate = new DateTimePicker();
            lblCalcTypeHeader = new Label();
            cboCalcType = new ComboBox();
            dataGrid = new DataGridView();
            clmNum = new DataGridViewTextBoxColumn();
            clmDate = new DataGridViewTextBoxColumn();
            clmInvoiceNum = new DataGridViewTextBoxColumn();
            clmDescription = new DataGridViewTextBoxColumn();
            clmWeight = new DataGridViewTextBoxColumn();
            clmQuantity = new DataGridViewTextBoxColumn();
            clmPrice = new DataGridViewTextBoxColumn();
            clmAmount = new DataGridViewTextBoxColumn();
            tableLayoutPanel1 = new TableLayoutPanel();
            lblSubTotalHeader = new Label();
            lblVATHeader = new Label();
            lblTotalHeader = new Label();
            lblSubTotal = new Label();
            lblVAT = new Label();
            lblTotal = new Label();
            flowLayoutPanel1 = new FlowLayoutPanel();
            btnGenerate = new Button();
            btnCalculate = new Button();
            btnAddRow = new Button();
            TableLayoutMain.SuspendLayout();
            tableLayoutHeader.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dataGrid).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            flowLayoutPanel1.SuspendLayout();
            SuspendLayout();
            // 
            // TableLayoutMain
            // 
            TableLayoutMain.ColumnCount = 1;
            TableLayoutMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            TableLayoutMain.Controls.Add(tableLayoutHeader, 0, 0);
            TableLayoutMain.Controls.Add(dataGrid, 0, 1);
            TableLayoutMain.Controls.Add(tableLayoutPanel1, 0, 2);
            TableLayoutMain.Controls.Add(flowLayoutPanel1, 0, 3);
            TableLayoutMain.Dock = DockStyle.Fill;
            TableLayoutMain.Location = new System.Drawing.Point(0, 0);
            TableLayoutMain.Name = "TableLayoutMain";
            TableLayoutMain.RowCount = 4;
            TableLayoutMain.RowStyles.Add(new RowStyle());
            TableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            TableLayoutMain.RowStyles.Add(new RowStyle());
            TableLayoutMain.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            TableLayoutMain.Size = new System.Drawing.Size(827, 593);
            TableLayoutMain.TabIndex = 2;
            // 
            // tableLayoutHeader
            // 
            tableLayoutHeader.ColumnCount = 4;
            tableLayoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            tableLayoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22F));
            tableLayoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 42F));
            tableLayoutHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 18F));
            tableLayoutHeader.Controls.Add(lblInvoiceNumHeader, 0, 0);
            tableLayoutHeader.Controls.Add(lblDateHeader, 1, 0);
            tableLayoutHeader.Controls.Add(lblVesselHeader, 2, 0);
            tableLayoutHeader.Controls.Add(lblInvoiceNum, 0, 1);
            tableLayoutHeader.Controls.Add(txtVessel, 2, 1);
            tableLayoutHeader.Controls.Add(dtpDate, 1, 1);
            tableLayoutHeader.Controls.Add(lblCalcTypeHeader, 3, 0);
            tableLayoutHeader.Controls.Add(cboCalcType, 3, 1);
            tableLayoutHeader.Dock = DockStyle.Fill;
            tableLayoutHeader.Location = new System.Drawing.Point(3, 3);
            tableLayoutHeader.Name = "tableLayoutHeader";
            tableLayoutHeader.RowCount = 2;
            tableLayoutHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutHeader.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutHeader.Size = new System.Drawing.Size(821, 46);
            tableLayoutHeader.TabIndex = 3;
            // 
            // lblInvoiceNumHeader
            // 
            lblInvoiceNumHeader.AutoSize = true;
            lblInvoiceNumHeader.Dock = DockStyle.Fill;
            lblInvoiceNumHeader.Location = new System.Drawing.Point(3, 0);
            lblInvoiceNumHeader.Name = "lblInvoiceNumHeader";
            lblInvoiceNumHeader.Size = new System.Drawing.Size(141, 23);
            lblInvoiceNumHeader.TabIndex = 0;
            lblInvoiceNumHeader.Text = "Invoice Number:";
            // 
            // lblDateHeader
            // 
            lblDateHeader.AutoSize = true;
            lblDateHeader.Dock = DockStyle.Fill;
            lblDateHeader.Location = new System.Drawing.Point(150, 0);
            lblDateHeader.Name = "lblDateHeader";
            lblDateHeader.Size = new System.Drawing.Size(174, 23);
            lblDateHeader.TabIndex = 1;
            lblDateHeader.Text = "Date:";
            // 
            // lblVesselHeader
            // 
            lblVesselHeader.AutoSize = true;
            lblVesselHeader.Dock = DockStyle.Fill;
            lblVesselHeader.Location = new System.Drawing.Point(330, 0);
            lblVesselHeader.Name = "lblVesselHeader";
            lblVesselHeader.Size = new System.Drawing.Size(338, 23);
            lblVesselHeader.TabIndex = 2;
            lblVesselHeader.Text = "Vessel:";
            // 
            // lblInvoiceNum
            // 
            lblInvoiceNum.AutoSize = true;
            lblInvoiceNum.Dock = DockStyle.Fill;
            lblInvoiceNum.Location = new System.Drawing.Point(3, 23);
            lblInvoiceNum.Name = "lblInvoiceNum";
            lblInvoiceNum.Size = new System.Drawing.Size(141, 23);
            lblInvoiceNum.TabIndex = 3;
            lblInvoiceNum.Text = "Num";
            // 
            // txtVessel
            // 
            txtVessel.Dock = DockStyle.Fill;
            txtVessel.Location = new System.Drawing.Point(330, 26);
            txtVessel.Name = "txtVessel";
            txtVessel.Size = new System.Drawing.Size(338, 23);
            txtVessel.TabIndex = 5;
            // 
            // dtpDate
            // 
            dtpDate.Location = new System.Drawing.Point(150, 26);
            dtpDate.Name = "dtpDate";
            dtpDate.Size = new System.Drawing.Size(174, 23);
            dtpDate.TabIndex = 6;
            // 
            // lblCalcTypeHeader
            // 
            lblCalcTypeHeader.AutoSize = true;
            lblCalcTypeHeader.Dock = DockStyle.Fill;
            lblCalcTypeHeader.Location = new System.Drawing.Point(674, 0);
            lblCalcTypeHeader.Name = "lblCalcTypeHeader";
            lblCalcTypeHeader.Size = new System.Drawing.Size(144, 23);
            lblCalcTypeHeader.TabIndex = 7;
            lblCalcTypeHeader.Text = "Calculation:";
            // 
            // cboCalcType
            // 
            cboCalcType.Dock = DockStyle.Fill;
            cboCalcType.DropDownStyle = ComboBoxStyle.DropDownList;
            cboCalcType.FormattingEnabled = true;
            cboCalcType.Items.AddRange(new object[] { "Weight", "Flat" });
            cboCalcType.Location = new System.Drawing.Point(674, 26);
            cboCalcType.Name = "cboCalcType";
            cboCalcType.Size = new System.Drawing.Size(144, 23);
            cboCalcType.TabIndex = 8;
            // 
            // dataGrid
            // 
            dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGrid.Columns.AddRange(new DataGridViewColumn[] { clmNum, clmDate, clmInvoiceNum, clmDescription, clmWeight, clmQuantity, clmPrice, clmAmount });
            dataGrid.Dock = DockStyle.Fill;
            dataGrid.Location = new System.Drawing.Point(3, 55);
            dataGrid.Name = "dataGrid";
            dataGrid.Size = new System.Drawing.Size(821, 379);
            dataGrid.TabIndex = 4;
            dataGrid.CellContentClick += dataGrid_CellContentClick;
            // 
            // clmNum
            // 
            clmNum.FillWeight = 80F;
            clmNum.HeaderText = "No.";
            clmNum.Name = "clmNum";
            clmNum.ReadOnly = true;
            // 
            // clmDate
            // 
            clmDate.FillWeight = 120F;
            clmDate.HeaderText = "DATE";
            clmDate.Name = "clmDate";
            // 
            // clmInvoiceNum
            // 
            clmInvoiceNum.FillWeight = 120F;
            clmInvoiceNum.HeaderText = "INVOICE No.";
            clmInvoiceNum.Name = "clmInvoiceNum";
            // 
            // clmDescription
            // 
            clmDescription.FillWeight = 300F;
            clmDescription.HeaderText = "DESCRIPTION";
            clmDescription.Name = "clmDescription";
            // 
            // clmWeight
            // 
            clmWeight.FillWeight = 120F;
            clmWeight.HeaderText = "WEIGHT";
            clmWeight.Name = "clmWeight";
            // 
            // clmQuantity
            // 
            clmQuantity.HeaderText = "QTY";
            clmQuantity.Name = "clmQuantity";
            // 
            // clmPrice
            // 
            clmPrice.FillWeight = 150F;
            clmPrice.HeaderText = "UNIT PRICE";
            clmPrice.Name = "clmPrice";
            // 
            // clmAmount
            // 
            clmAmount.FillWeight = 150F;
            clmAmount.HeaderText = "AMOUNT";
            clmAmount.Name = "clmAmount";
            clmAmount.ReadOnly = true;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Controls.Add(lblSubTotalHeader, 0, 0);
            tableLayoutPanel1.Controls.Add(lblVATHeader, 0, 1);
            tableLayoutPanel1.Controls.Add(lblTotalHeader, 0, 2);
            tableLayoutPanel1.Controls.Add(lblSubTotal, 1, 0);
            tableLayoutPanel1.Controls.Add(lblVAT, 1, 1);
            tableLayoutPanel1.Controls.Add(lblTotal, 1, 2);
            tableLayoutPanel1.Dock = DockStyle.Right;
            tableLayoutPanel1.Location = new System.Drawing.Point(624, 440);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 3;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 33.3333321F));
            tableLayoutPanel1.Size = new System.Drawing.Size(200, 100);
            tableLayoutPanel1.TabIndex = 5;
            // 
            // lblSubTotalHeader
            // 
            lblSubTotalHeader.AutoSize = true;
            lblSubTotalHeader.Dock = DockStyle.Fill;
            lblSubTotalHeader.Location = new System.Drawing.Point(3, 0);
            lblSubTotalHeader.Name = "lblSubTotalHeader";
            lblSubTotalHeader.Size = new System.Drawing.Size(94, 33);
            lblSubTotalHeader.TabIndex = 0;
            lblSubTotalHeader.Text = "Subtotal:";
            // 
            // lblVATHeader
            // 
            lblVATHeader.AutoSize = true;
            lblVATHeader.Dock = DockStyle.Fill;
            lblVATHeader.Location = new System.Drawing.Point(3, 33);
            lblVATHeader.Name = "lblVATHeader";
            lblVATHeader.Size = new System.Drawing.Size(94, 33);
            lblVATHeader.TabIndex = 1;
            lblVATHeader.Text = "VAT:";
            // 
            // lblTotalHeader
            // 
            lblTotalHeader.AutoSize = true;
            lblTotalHeader.Dock = DockStyle.Fill;
            lblTotalHeader.Location = new System.Drawing.Point(3, 66);
            lblTotalHeader.Name = "lblTotalHeader";
            lblTotalHeader.Size = new System.Drawing.Size(94, 34);
            lblTotalHeader.TabIndex = 2;
            lblTotalHeader.Text = "Total: ";
            // 
            // lblSubTotal
            // 
            lblSubTotal.AutoSize = true;
            lblSubTotal.Dock = DockStyle.Fill;
            lblSubTotal.Location = new System.Drawing.Point(103, 0);
            lblSubTotal.Name = "lblSubTotal";
            lblSubTotal.Size = new System.Drawing.Size(94, 33);
            lblSubTotal.TabIndex = 3;
            lblSubTotal.Text = "R xx,xx";
            // 
            // lblVAT
            // 
            lblVAT.AutoSize = true;
            lblVAT.Dock = DockStyle.Fill;
            lblVAT.Location = new System.Drawing.Point(103, 33);
            lblVAT.Name = "lblVAT";
            lblVAT.Size = new System.Drawing.Size(94, 33);
            lblVAT.TabIndex = 4;
            lblVAT.Text = "R xx,xx";
            // 
            // lblTotal
            // 
            lblTotal.AutoSize = true;
            lblTotal.Dock = DockStyle.Fill;
            lblTotal.Location = new System.Drawing.Point(103, 66);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new System.Drawing.Size(94, 34);
            lblTotal.TabIndex = 5;
            lblTotal.Text = "R xx,xx";
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Controls.Add(btnGenerate);
            flowLayoutPanel1.Controls.Add(btnCalculate);
            flowLayoutPanel1.Controls.Add(btnAddRow);
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.FlowDirection = FlowDirection.RightToLeft;
            flowLayoutPanel1.Location = new System.Drawing.Point(3, 546);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new System.Drawing.Size(821, 44);
            flowLayoutPanel1.TabIndex = 6;
            // 
            // btnGenerate
            // 
            btnGenerate.Location = new System.Drawing.Point(547, 3);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new System.Drawing.Size(271, 32);
            btnGenerate.TabIndex = 0;
            btnGenerate.Text = "Generate Invoice";
            btnGenerate.UseVisualStyleBackColor = true;
            btnGenerate.Click += btnGenerate_Click;
            // 
            // btnCalculate
            // 
            btnCalculate.Location = new System.Drawing.Point(416, 3);
            btnCalculate.Name = "btnCalculate";
            btnCalculate.Size = new System.Drawing.Size(125, 32);
            btnCalculate.TabIndex = 1;
            btnCalculate.Text = "Calculate";
            btnCalculate.UseVisualStyleBackColor = true;
            // 
            // btnAddRow
            // 
            btnAddRow.Location = new System.Drawing.Point(266, 3);
            btnAddRow.Name = "btnAddRow";
            btnAddRow.Size = new System.Drawing.Size(144, 32);
            btnAddRow.TabIndex = 2;
            btnAddRow.Text = "Add Row";
            btnAddRow.UseVisualStyleBackColor = true;
            // 
            // InvoiceBuilder
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(827, 593);
            Controls.Add(TableLayoutMain);
            Name = "InvoiceBuilder";
            Text = "Invoice Builder";
            TableLayoutMain.ResumeLayout(false);
            tableLayoutHeader.ResumeLayout(false);
            tableLayoutHeader.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dataGrid).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            tableLayoutPanel1.PerformLayout();
            flowLayoutPanel1.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private TableLayoutPanel TableLayoutMain;
        private TableLayoutPanel tableLayoutHeader;
        private Label lblInvoiceNumHeader;
        private Label lblDateHeader;
        private Label lblVesselHeader;
        private Label lblInvoiceNum;
        private TextBox txtVessel;
        private DataGridView dataGrid;
        private DataGridViewTextBoxColumn clmNum;
        private DataGridViewTextBoxColumn clmDate;
        private DataGridViewTextBoxColumn clmInvoiceNum;
        private DataGridViewTextBoxColumn clmDescription;
        private DataGridViewTextBoxColumn clmWeight;
        private DataGridViewTextBoxColumn clmQuantity;
        private DataGridViewTextBoxColumn clmPrice;
        private DataGridViewTextBoxColumn clmAmount;
        private TableLayoutPanel tableLayoutPanel1;
        private Label lblSubTotalHeader;
        private Label lblVATHeader;
        private Label lblTotalHeader;
        private Label lblSubTotal;
        private Label lblVAT;
        private Label lblTotal;
        private FlowLayoutPanel flowLayoutPanel1;
        private Button btnGenerate;
        private Button btnCalculate;
        private Button btnAddRow;
        private DateTimePicker dtpDate;
        private Label lblCalcTypeHeader;
        private ComboBox cboCalcType;
    }
}
