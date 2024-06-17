using Microsoft.Data.Sqlite;

namespace storageManager
{
    public partial class Form1 : Form
    {
        internal static string connectionString;
        private SqliteConnection _connection = null;
        public Form1()
        {
            InitializeComponent();
            textBox2.PasswordChar = '*';
        }

        async private void Form1_Load(object sender, EventArgs e)
        {
            using (var dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.Title = "Выберите файл БД .db";
                dlg.Filter = "Файл базы данных (*.db)|*.db";
                dlg.CheckFileExists = true;

                if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string dbFile = dlg.FileName;

                    connectionString = Program.connection.Replace("{DB}", dbFile);
                    Program.connection = connectionString;
                }
                else
                {
                    this.Close();
                }
            }
            try
            {
                _connection = new SqliteConnection(Program.connection);
                await _connection.OpenAsync();
                MessageBox.Show("Подключение успешно!\n" +
                    connectionString, "Выбор файла БД", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && !string.IsNullOrWhiteSpace(textBox1.Text) &&
                !string.IsNullOrEmpty(textBox2.Text) && !string.IsNullOrWhiteSpace(textBox2.Text))
            {
                SqliteCommand cmdGetUser = new SqliteCommand("SELECT * FROM Users WHERE [username]=@username", _connection);
                cmdGetUser.Parameters.AddWithValue("username", textBox1.Text);
                using (var reader = cmdGetUser.ExecuteReader())
                {
                    reader.Read();
                    string currUser = reader["username"].ToString();
                    if (Encoder.VerifyHash(textBox2.Text, "SHA512", reader["pass_hash"].ToString()))
                    {
                        var result = DialogResult.Cancel;
                        if (reader["access_level"].ToString() == "admin")
                        {
                            result = MessageBox.Show("Добро пожаловать, администратор!\n"
                            , "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            result = MessageBox.Show("Добро пожаловать!"
                            , "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        if (result == DialogResult.OK)
                        {
                            reader.Close();
                            _connection.Close();
                            this.Hide();
                            var form2 = new Form2(currUser);
                            form2.Closed += (s, args) => this.Close();
                            form2.Show();
                        }
                    }
                    else
                    {
                        reader.Close();
                        MessageBox.Show("Неверно введены данные пользователя!", "Авторизация", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
