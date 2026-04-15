using System;
using System.Drawing;
using System.Windows.Forms;

namespace CineMatch
{
    public class FormLogin : Form
    {
        private Panel pnlLogin, pnlRegister;
        private TextBox txtUser, txtPass;
        private TextBox txtRegName, txtRegEmail, txtRegUser, txtRegPass;
        private DateTimePicker dtpDob;
        private ComboBox cboGenre;
        private Button btnLogin, btnRegister, btnGoReg, btnGoLogin;
        private Label lblError;

        public FormLogin()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            Text = "CineMatch - Sign In";
            Size = new Size(440, 560);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            BackColor = Color.FromArgb(9, 9, 15);
            ForeColor = Color.FromArgb(238, 232, 213);

            var lblBrand = new Label
            {
                Text = "CineMatch",
                Font = new Font("Segoe UI", 26, FontStyle.Bold),
                ForeColor = Color.FromArgb(232, 197, 71),
                AutoSize = true,
                Location = new Point(130, 24),
            };
            Controls.Add(lblBrand);

            var lblSub = new Label
            {
                Text = "// Hybrid Movie Recommendation System",
                Font = new Font("Courier New", 7),
                ForeColor = Color.FromArgb(80, 80, 100),
                AutoSize = true,
                Location = new Point(100, 72),
            };
            Controls.Add(lblSub);

            btnGoLogin = MakeTabBtn("Sign In", new Point(30, 98));
            btnGoReg = MakeTabBtn("Register", new Point(215, 98));
            btnGoLogin.Click += (s, e) => ShowPanel(true);
            btnGoReg.Click += (s, e) => ShowPanel(false);
            Controls.Add(btnGoLogin);
            Controls.Add(btnGoReg);

            lblError = new Label
            {
                ForeColor = Color.FromArgb(232, 82, 82),
                Font = new Font("Courier New", 8),
                AutoSize = false,
                Size = new Size(370, 20),
                Location = new Point(30, 470),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            Controls.Add(lblError);

            var hint = new Label
            {
                Text = "Demo: arjun / pass123  .  priya / pass123  .  rahul / pass123",
                ForeColor = Color.FromArgb(80, 80, 100),
                Font = new Font("Courier New", 7),
                AutoSize = false,
                Size = new Size(370, 40),
                Location = new Point(30, 493),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            Controls.Add(hint);

            //Login panel 
            pnlLogin = new Panel
            {
                Location = new Point(30, 130),
                Size = new Size(370, 320),
                BackColor = Color.Transparent
            };
            pnlLogin.Controls.Add(MakeLabel("Username", 0));
            txtUser = MakeInput(28);
            pnlLogin.Controls.Add(txtUser);
            pnlLogin.Controls.Add(MakeLabel("Password", 68));
            txtPass = MakeInput(96);
            txtPass.UseSystemPasswordChar = true;
            pnlLogin.Controls.Add(txtPass);
            btnLogin = MakeGoldBtn("Sign In", new Point(0, 148));
            btnLogin.Click += BtnLogin_Click;
            pnlLogin.Controls.Add(btnLogin);
            Controls.Add(pnlLogin);

            // Register panel
            pnlRegister = new Panel
            {
                Location = new Point(30, 130),
                Size = new Size(370, 340),
                BackColor = Color.Transparent,
                Visible = false
            };

            int y = 0;
            pnlRegister.Controls.Add(MakeLabel("Full Name", y));
            txtRegName = MakeInput(y + 18);
            pnlRegister.Controls.Add(txtRegName);
            y += 52;

            pnlRegister.Controls.Add(MakeLabel("Email", y));
            txtRegEmail = MakeInput(y + 18);
            pnlRegister.Controls.Add(txtRegEmail);
            y += 52;

            pnlRegister.Controls.Add(MakeLabel("Username", y));
            txtRegUser = MakeInput(y + 18);
            pnlRegister.Controls.Add(txtRegUser);
            y += 52;

            pnlRegister.Controls.Add(MakeLabel("Password (min 6 chars)", y));
            txtRegPass = MakeInput(y + 18);
            txtRegPass.UseSystemPasswordChar = true;
            pnlRegister.Controls.Add(txtRegPass);
            y += 52;

            pnlRegister.Controls.Add(MakeLabel("Date of Birth", y));
            dtpDob = new DateTimePicker
            {
                Location = new Point(0, y + 18),
                Size = new Size(370, 28),
                Format = DateTimePickerFormat.Short,
                Value = DateTime.Today.AddYears(-20),
                MaxDate = DateTime.Today.AddYears(-13),
            };
            pnlRegister.Controls.Add(dtpDob);
            y += 52;

            pnlRegister.Controls.Add(MakeLabel("Favourite Genre", y));
            cboGenre = new ComboBox
            {
                Location = new Point(0, y + 18),
                Size = new Size(370, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(25, 25, 36),
                ForeColor = Color.FromArgb(238, 232, 213),
                FlatStyle = FlatStyle.Flat,
            };
            cboGenre.Items.AddRange(new object[]
            {
                "Drama","Action","Comedy","Horror","Sci-Fi",
                "Thriller","Romance","Animation","Crime","Fantasy","Biography","Musical"
            });
            cboGenre.SelectedIndex = 0;
            pnlRegister.Controls.Add(cboGenre);
            y += 52;

            btnRegister = MakeGoldBtn("Create Account", new Point(0, y));
            btnRegister.Click += BtnRegister_Click;
            pnlRegister.Controls.Add(btnRegister);
            Controls.Add(pnlRegister);

            KeyPreview = true;
            KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    if (pnlLogin.Visible)
                        BtnLogin_Click(s, e);
                    else
                        BtnRegister_Click(s, e);
                }
            };

            ShowPanel(true);
        }

        //Show login or register panel
        private void ShowPanel(bool login)
        {
            pnlLogin.Visible = login;
            pnlRegister.Visible = !login;
            lblError.Text = "";

            btnGoLogin.BackColor = login
                ? Color.FromArgb(232, 197, 71)
                : Color.FromArgb(25, 25, 36);
            btnGoLogin.ForeColor = login
                ? Color.Black
                : Color.FromArgb(180, 180, 180);

            btnGoReg.BackColor = !login
                ? Color.FromArgb(232, 197, 71)
                : Color.FromArgb(25, 25, 36);
            btnGoReg.ForeColor = !login
                ? Color.Black
                : Color.FromArgb(180, 180, 180);
        }

        //Login button click
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text;

            if (string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblError.Text = "Please fill in all fields.";
                return;
            }

            try
            {
                User u = DataService.Login(user, pass);
                if (u == null)
                {
                    lblError.Text = "Incorrect username or password.";
                    return;
                }

                var main = new FormMain(u);
                main.Show();
                Hide();
                main.FormClosed += (s2, e2) => Close();
            }
            catch (Exception ex)
            {
                lblError.Text = "DB Error: " + ex.Message;
            }
        }

        //Register button click
        private void BtnRegister_Click(object sender, EventArgs e)
        {
            lblError.Text = "";
            string name = txtRegName.Text.Trim();
            string email = txtRegEmail.Text.Trim();
            string user = txtRegUser.Text.Trim();
            string pass = txtRegPass.Text;
            string genre = cboGenre.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(user) || string.IsNullOrEmpty(pass))
            {
                lblError.Text = "Please fill in all required fields.";
                return;
            }

            if (pass.Length < 6)
            {
                lblError.Text = "Password must be at least 6 characters.";
                return;
            }

            try
            {
                DataService.Register(name, email, user, pass, dtpDob.Value, genre);
                User u = DataService.Login(user, pass);
                MessageBox.Show("Welcome, " + name.Split(' ')[0] + "! Account created.",
                                "CineMatch", MessageBoxButtons.OK, MessageBoxIcon.Information);
                var main = new FormMain(u);
                main.Show();
                Hide();
                main.FormClosed += (s2, e2) => Close();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Duplicate entry"))
                    lblError.Text = "Email or username already taken.";
                else
                    lblError.Text = "Error: " + ex.Message;
            }
        }

        //Control factory helpers
        private Label MakeLabel(string text, int y)
        {
            return new Label
            {
                Text = text.ToUpper(),
                Font = new Font("Courier New", 7),
                ForeColor = Color.FromArgb(100, 100, 130),
                AutoSize = true,
                Location = new Point(0, y),
            };
        }

        private TextBox MakeInput(int y)
        {
            return new TextBox
            {
                Location = new Point(0, y),
                Size = new Size(370, 28),
                BackColor = Color.FromArgb(25, 25, 36),
                ForeColor = Color.FromArgb(238, 232, 213),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
            };
        }

        private Button MakeGoldBtn(string text, Point loc)
        {
            return new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(370, 40),
                BackColor = Color.FromArgb(232, 197, 71),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
        }

        private Button MakeTabBtn(string text, Point loc)
        {
            return new Button
            {
                Text = text,
                Location = loc,
                Size = new Size(175, 32),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
        }
    }
}