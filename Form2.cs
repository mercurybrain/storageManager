using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Windows.Forms.AxHost;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace storageManager
{
    public partial class Form2 : Form
    {
        string currentUser = "";
        private SqliteConnection? _connection = null;
        private DataTable orders = new DataTable();
        private DataTable users = new DataTable();
        private DataTable floors = new DataTable();
        private DataTable tiles = new DataTable();
        private DataTable roof = new DataTable();
        private DataTable instruments = new DataTable();
        private DataTable paints = new DataTable();
        private DataTable sharedTypes = new DataTable();
        private DataTable typesSearch = new DataTable();
        private DataTable bmaterials = new DataTable();
        private DataTable log = new DataTable();
        private DataTable brands = new DataTable();
        private string format = "dd.MM.yyyy HH:mm:ss";

        List<string> itemsToAdd = new List<string>();
        float totalPrice = 0;
        Dictionary<string, string> tables = new Dictionary<string, string>() { { "Не указан", "Не указан" } };

        public Form2(string currentUser)
        {
            InitializeComponent();
            this.currentUser = currentUser;
        }
        async private void Form2_Load(object sender, EventArgs e)
        {
            try
            {
                _connection = new SqliteConnection(Program.connection);
                await _connection.OpenAsync();
                await GetTableParams();

                GetData(orders, "orders", ordersDGV, true);
                GetData(sharedTypes, "sharedTypes", sharedTypesDGV, true);
                GetData(typesSearch, "sharedTypes");
                GetData(instruments, "instruments", instrumentsDGV, true);
                GetData(brands, "sharedCategories", brandsDGV, true);
                GetData(floors, "floors", floorsDGV, true);
                GetData(tiles, "tiles", tilesDGV, true);
                GetData(roof, "roofs", roofsDGV, true);
                GetData(bmaterials, "bmaterials", bmatsDGV1, true);
                GetData(paints, "paints", paintsDGV, true);
                GetData(users, "users", usersDGV, true);
                GetData(log, "log", logsDGV, true);

                PutCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        async private Task GetTableParams()
        {
            using (var command = new SqliteCommand("SELECT type, typeTable FROM sharedTypes", _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    var dataDict = new Dictionary<string, string>();
                    while (reader.Read())
                    {
                        string type = reader.GetString(0);
                        string typeTable = reader.GetString(1);
                        dataDict[type] = typeTable;
                    }
                    tables = dataDict;
                }
            }
        }
        public bool ValidateControl(Control ctrl)
        {
            if (!string.IsNullOrEmpty(ctrl.Text) && !string.IsNullOrWhiteSpace(ctrl.Text)) return true;
            return false;
        }
        public bool ValidateControls(System.Windows.Forms.Control.ControlCollection ctrls)
        {
            var textBoxes = ctrls.OfType<System.Windows.Forms.TextBox>()
                .Where(textBox => string.IsNullOrEmpty(textBox.Text));

            var comboBoxes = ctrls.OfType<System.Windows.Forms.ComboBox>()
                .Where(comboBox => comboBox.SelectedIndex == -1);

            if (textBoxes.Any() || comboBoxes.Any()) return false;

            return true;
        }

        private void GetData(DataTable table, string name, DataGridView? dgv = null, bool initial = false)
        {
            var getData = new SqliteCommand("SELECT rowid, * FROM '" + name + "'", _connection);
            table.Clear();
            table.Load(getData.ExecuteReader());
            if (dgv != null)
            {
                dgv.DataSource = table;
                /*if (Program.accessLevel == "Администратор" || Program.accessLevel == "Менеджер")
                {
                    putUninstall(dgv);
                    if (initial)
                    {
                        dgv.CellClick += delegate (object sender, DataGridViewCellEventArgs e) { onCellClick(sender, e, name, dgv); };
                        dgv.CellEndEdit += delegate (object sender, DataGridViewCellEventArgs e) { onCellEdit(sender, e, name, dgv); };
                    }
                }*/
                putUninstall(dgv);
                if (initial)
                {
                    dgv.CellClick += delegate (object sender, DataGridViewCellEventArgs e) { onCellClick(sender, e, name, dgv); };
                    dgv.CellEndEdit += delegate (object sender, DataGridViewCellEventArgs e) { onCellEdit(sender, e, name, dgv); };
                }
                placeNames(name, dgv);
            }
        }
        private async void UpdateData()
        {
            try
            {
                GetData(orders, "orders", ordersDGV);
                GetData(sharedTypes, "sharedTypes", sharedTypesDGV);
                GetData(typesSearch, "sharedTypes");
                GetData(instruments, "instruments", instrumentsDGV);
                GetData(brands, "sharedCategories", brandsDGV);
                GetData(floors, "floors", floorsDGV);
                GetData(tiles, "tiles", tilesDGV);
                GetData(roof, "roofs", roofsDGV);
                GetData(bmaterials, "bmaterials", bmatsDGV1);
                GetData(paints, "paints", paintsDGV);
                GetData(log, "log", logsDGV);
                GetData(users, "users", usersDGV);
                PutCombo();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\nТекущая строка подлючения: " + Program.connection, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void placeNames(string name, DataGridView dgv)
        {
            dgv.Columns[0].DefaultCellStyle.Format = "n0";
            var names = new Dictionary<string, string>()
            {
                { "rowid", "ID записи" },
                { "name", "Наименование" },
                { "articul", "Артикул" },
                { "datetime", "Время оформления заказа" },
                { "what", "Позиции" },
                { "totalsum", "Общая стоимость" },
                { "username", "Имя пользователя" },
                { "workpartMaterial", "Материал рабочей части" },
                { "handleMaterial", "Материал рукоятки" },
                { "weight", "Вес" },
                { "length", "Длина" },
                { "price", "Цена за штуку/упаковку" },
                { "priceMeters", "Цена за м^2" },
                { "type", "Тип" },
                { "country", "Страна" },
                { "subtype", "Подтип" },
                { "typeTable", "Связанная таблица" },
                { "width", "Ширина" },
                { "brand", "Производитель" },
                { "inPackage", "Сколько товара в упаковке(шт/м^2/л"},
                { "washable", "Можно мыть"},
                { "desc", "Примечания"},
                { "description", "Примечания"},
                { "destination", "Назначение"},
                { "workMaterial", "Обрабатываемый материал"},
                { "deliveryAddr", "Адрес доставки"},
                { "isDelivery", "Нужна доставка"}
            };
            foreach (DataGridViewColumn column in dgv.Columns)
            {
                if (names.ContainsKey(column.HeaderText)) column.HeaderText = names[column.HeaderText];
            }
        }
        private void PutCombo()
        {

            categoryCombo1.Items.Clear();
            categoryCombo2.Items.Clear();
            categoryCombo3.Items.Clear();
            categoryCombo1.Items.AddRange(tables.Keys.Distinct().ToArray());
            categoryCombo3.Items.AddRange(tables.Keys.Distinct().ToArray());
            categoryCombo2.Items.AddRange(tables.Keys.Distinct().ToArray());


            addInstrumentBrand.DisplayMember = "brand";
            addInstrumentBrand.ValueMember = "brand";
            addInstrumentBrand.DataSource = brands;

            addFloorBrand.DisplayMember = "brand";
            addFloorBrand.ValueMember = "brand";
            addFloorBrand.DataSource = brands;

            addTilesBrand.DisplayMember = "brand";
            addTilesBrand.ValueMember = "brand";
            addTilesBrand.DataSource = brands;

            addRoofBrand.DisplayMember = "brand";
            addRoofBrand.ValueMember = "brand";
            addRoofBrand.DataSource = brands;

            addBmatBrand.DisplayMember = "brand";
            addBmatBrand.ValueMember = "brand";
            addBmatBrand.DataSource = brands;

            paintaddBrand.DisplayMember = "brand";
            paintaddBrand.ValueMember = "brand";
            paintaddBrand.DataSource = brands;


            addInstrumentType.Items.AddRange(GetSubtypes("Инструменты"));

            addFloorType.Items.AddRange(GetSubtypes("Напольные покрытия"));

            addTilesType.Items.AddRange(GetSubtypes("Плитка"));

            addRoofType.Items.AddRange(GetSubtypes("Кровля"));

            addBmatType.Items.AddRange(GetSubtypes("Строительные материалы"));

            paintaddType.Items.AddRange(GetSubtypes("Краски"));

        }

        private string?[] GetSubtypes(string type)
        {
            return sharedTypes.AsEnumerable()
                .Where(row => row.Field<string>("type") == type)
                .Select(row => row.Field<string>("subType"))
                .ToArray();
        }
        private void putUninstall(DataGridView dgv)
        {
            DataGridViewButtonColumn uninstallButtonColumn = new DataGridViewButtonColumn();
            uninstallButtonColumn.Name = "Удалить";
            uninstallButtonColumn.Text = "Удалить";
            uninstallButtonColumn.UseColumnTextForButtonValue = true;
            int columnIndex = dgv.Columns.Count;
            if (dgv.Columns["Удалить"] == null)
            {
                dgv.Columns.Insert(columnIndex, uninstallButtonColumn);
            }
        }
        private async void onCellClick(object sender, DataGridViewCellEventArgs e, string name, DataGridView dgv)
        {
            if (e.ColumnIndex == dgv.Columns["Удалить"].Index)
            {
                DataGridViewRow row = dgv.Rows[e.RowIndex];
                string id = row.Cells["rowid"].Value.ToString();
                var result = MessageBox.Show("Вы уверены, что хотите удалить запись ID: " + id +
                    " из таблицы " + name + " без возможности восстановления?\nЭто так же удалит все связанные записи в других таблицах!"
                    , "Подтверждение", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.OK)
                {
                    SqliteCommand delete = new SqliteCommand("DELETE FROM '" + name + "' WHERE rowid=@Id", _connection);
                    delete.Parameters.AddWithValue("Id", id);

                    await delete.ExecuteNonQueryAsync();
                    if (dgv.Name != "logsDGV") PutLog(delete.CommandText, "DELETE " + name, new string[] { id });
                    UpdateData();
                }
            }
        }
        private async void onCellEdit(object sender, DataGridViewCellEventArgs e, string name, DataGridView dgv)
        {
            if (dgv.Name == "usersDGV")
            {
                MessageBox.Show("Редактировать данные пользователей запрещено во избежание повреждения хэша пароля.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                //GetData(new DataTable(), "Users", usersDGV);
                return;
            }
            else
            {
                DataGridViewRow row = dgv.Rows[e.RowIndex];
                string id = row.Cells["rowid"].Value.ToString();
                Dictionary<string, string> cells = new Dictionary<string, string>();
                string updateCommandText = "UPDATE " + name + " SET";
                for (int i = 1; i < row.Cells.Count; i++)
                {
                    cells.Add(row.Cells[i].OwningColumn.Name, row.Cells[i].Value.ToString());
                }
                foreach (var cell in cells)
                {
                    if (cell.Value != "Удалить")
                    {
                        updateCommandText += " " + cell.Key + "=" + "'" + cell.Value + "'" + ",";
                    }
                }
                if (updateCommandText[updateCommandText.Length - 1] == ',')
                {
                    updateCommandText = updateCommandText.Substring(0, updateCommandText.Length - 1);
                }
                updateCommandText += " WHERE rowid=" + id;
                try
                {
                    await new SqliteCommand(updateCommandText, _connection).ExecuteNonQueryAsync();
                    if (dgv.Name != "logsDGV") PutLog(updateCommandText, "UPDATE " + name, new string[] { id });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                UpdateData();
            }
        }
        private void PutLog(string commandText, string fromWhere, string[] parameters = null)
        {
            SqliteCommand putLog = new SqliteCommand("INSERT INTO log(text, fromWhere, user, parameters, operation_time)VALUES(@text, @fromWhere, @user, @parameters, @operation_time)", _connection);
            putLog.Parameters.AddWithValue("text", commandText);
            putLog.Parameters.AddWithValue("fromWhere", "'" + fromWhere + "'");
            string parametersLine = "";
            if (parameters != null)
            {
                foreach (string param in parameters) parametersLine += (param + ";");
            }
            else parametersLine = "Аргументы приведены в тексте команды";
            putLog.Parameters.AddWithValue("parameters", parametersLine);
            putLog.Parameters.AddWithValue("user", currentUser);
            putLog.Parameters.AddWithValue("operation_time", DateTime.Now.ToString(format));

            putLog.ExecuteNonQuery();
        }

        public void InsertRecord(string tableName, Dictionary<string, object> parameters)
        {
            string columns = string.Join(", ", parameters.Keys);
            string values = string.Join(", ", parameters.Keys.Select(x => $"@{x}"));
            List<string> logList = new List<string>();

            string sql = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

            using (SqliteCommand command = new SqliteCommand(sql, _connection))
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue($"@{parameter.Key}", parameter.Value);
                    logList.Add(parameter.Key + parameter.Value);
                }

                try
                {
                    command.ExecuteNonQuery();
                    PutLog(command.CommandText, $"INSERT {tableName}", logList.ToArray());
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, ex.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        Dictionary<string, string> GetDataAsDictionary(string tableName, string[] columns, string whereClause = null)
        {
            string queryString = $"SELECT {string.Join(", ", columns)} FROM {tableName}";
            if (!string.IsNullOrEmpty(whereClause))
            {
                queryString += $" WHERE {whereClause}";
            }

            using (var command = new SqliteCommand(queryString, _connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    var dataDict = new Dictionary<string, string>();
                    while (reader.Read())
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            string columnName = columns[i];
                            string value = reader.GetString(i);
                            dataDict[columnName] = value;
                        }
                    }
                    return dataDict;
                }
            }
        }
        private void toolStripButton1_Click_2(object sender, EventArgs e)
        {
            UpdateData();
        }
        private void categoryCombo1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SqliteCommand getSubtypes = new SqliteCommand("SELECT * FROM sharedTypes WHERE type=@type", _connection);
            getSubtypes.Parameters.AddWithValue("type", categoryCombo1.Text);
            DataTable temp = new DataTable();
            temp.Clear();
            temp.Load(getSubtypes.ExecuteReader());

            subcategoryCombo1.DisplayMember = "subtype";
            subcategoryCombo1.ValueMember = "subtype";
            subcategoryCombo1.DataSource = temp;


        }

        private void categoryCombo2_SelectedIndexChanged(object sender, EventArgs e)
        {
            SqliteCommand getSubtypes = new SqliteCommand("SELECT * FROM sharedTypes WHERE type=@type", _connection);
            getSubtypes.Parameters.AddWithValue("type", categoryCombo2.Text);
            DataTable temp = new DataTable();
            temp.Clear();
            temp.Load(getSubtypes.ExecuteReader());

            subcategoryCombo2.DisplayMember = "subtype";
            subcategoryCombo2.ValueMember = "subtype";
            subcategoryCombo2.DataSource = temp;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (ValidateControl(categoryCombo3) && ValidateControl(addSubtypeText))
            {
                InsertRecord("sharedTypes", new Dictionary<string, object>()
                {
                    { "subtype", addSubtypeText.Text },
                    { "type", categoryCombo3.Text },
                    { "typeTable", addSubtypeTable.Text}
                });
                UpdateData();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            dateTimePicker1.Value = DateTime.Now;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (ValidateControl(itemCombo1) && ValidateControl(kolvoTextbox1))
            {
                float kolvo = float.Parse(kolvoTextbox1.Text, CultureInfo.InvariantCulture);
                using (SqliteCommand getPrice = new SqliteCommand($"SELECT {(quantityCombo.Text == "шт." ? "price" : "priceMeters")}" +
                    $" FROM {tables[categoryCombo1.Text]} WHERE name='{itemCombo1.Text}'", _connection))
                {
                    using (var reader = getPrice.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            float price = float.Parse(reader.GetString(0), CultureInfo.InvariantCulture);
                            totalPrice += price * kolvo;
                        }
                    }
                }

                itemsToAdd.Add(itemCombo1.Text + " : " + kolvoTextbox1.Text + quantityCombo.Text + ", ");
                MessageBox.Show(totalPrice.ToString() + " : " + string.Join(" ", itemsToAdd.ToArray()));
                kolvoTextbox1.Text = string.Empty;
            }
            else
            {
                MessageBox.Show("Указаны не все данные для добавления позиции в заказ!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            itemsToAdd = new List<string>();
            totalPrice = 0;
            MessageBox.Show("Формирование заказа прекращено.", "Формирование заказа", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void addOrderClick(object sender, EventArgs e)
        {
            var parameters = new Dictionary<string, object>();
            string resItems = "";
            if (itemsToAdd.Count() != 0)
            {

                resItems = string.Join("", itemsToAdd.ToArray()).Trim();
                resItems = resItems.Remove(resItems.Length - 1);
                parameters.Add("what", resItems);
                if (ValidateControl(dateTimePicker1)) parameters.Add("datetime", dateTimePicker1.Text);
                else MessageBox.Show("Не указано время заказа!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                parameters.Add("totalsum", totalPrice.ToString());
                MessageBox.Show(currentUser);
                parameters.Add("username", currentUser);
                if (addOrderIsDelivery.Checked && ValidateControl(addOrderAddr))
                {
                    parameters.Add("isDelivery", "Да");
                    parameters.Add("deliveryAddr", addOrderAddr.Text);
                }
                if (ValidateControl(addOrderDesc)) parameters.Add("desc", addOrderDesc.Text);

                try
                {
                    InsertRecord("orders", parameters);
                    UpdateData();

                    itemsToAdd.Clear();
                    totalPrice = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Необходимо добавить хотя бы одну позицию!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (ValidateControls(tabPage7.Controls))
            {
                try
                {
                    InsertRecord("instruments", new Dictionary<string, object>()
                {
                    { "name", addInstrumentNameText.Text },
                    { "articul", addInstrumentArticul.Text },
                    { "workpartMaterial", addInstrumentWorkpart.Text },
                    { "handleMaterial", addInstrumentHandle.Text },
                    { "weight", float.Parse(addInstrumentWEight.Text, CultureInfo.InvariantCulture) },
                    { "length", float.Parse(addInstrumentLength.Text, CultureInfo.InvariantCulture) },
                    { "price", float.Parse(addInstrumentPrice.Text, CultureInfo.InvariantCulture)},
                    { "type", addInstrumentType.Text },
                    { "brand", addInstrumentBrand.Text},
                    { "workMaterial", addInstrumentMat.Text},
                    {"desc", addInstrumentDesc.Text}
                });
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Заполнены не все обязательные поля.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            if (ValidateControls(tabPage13.Controls))
            {
                try
                {
                    InsertRecord("floors", new Dictionary<string, object>()
                {
                    { "name", addFloorName.Text },
                    { "articul", addFloorArticle.Text },
                    { "price", float.Parse(addFloorpricepackage.Text, CultureInfo.InvariantCulture)},
                    { "priceMeters", float.Parse(addFloorPriceMeters.Text, CultureInfo.InvariantCulture)},
                    { "inPackage", float.Parse(addFloorInPackage.Text, CultureInfo.InvariantCulture)},
                    { "description", addFloorDesc.Text},
                    { "type", addFloorType.Text },
                    { "brand", addFloorBrand.Text }
                });
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Заполнены не все обязательные поля.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (ValidateControls(tabPage9.Controls))
            {
                try
                {
                    InsertRecord("sharedCategories", new Dictionary<string, object>()
                {
                    { "brand", textBox2.Text },
                    { "country", textBox1.Text },
                });
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Заполнены не все обязательные поля.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (ValidateControls(tabPage15.Controls))
            {
                try
                {
                    InsertRecord("tiles", new Dictionary<string, object>()
                {
                    { "name", addTIlesName.Text },
                    { "articul", addTilesArticle.Text },
                    { "description", addTilesDesc.Text },
                    { "price", float.Parse(addTilesPricePackage.Text, CultureInfo.InvariantCulture)},
                    { "priceMeters", float.Parse(addTilesPriceMeters.Text, CultureInfo.InvariantCulture)},
                    { "inPackage", float.Parse(addTilesInPackage.Text, CultureInfo.InvariantCulture)},
                    { "type", addTilesType.Text },
                    { "brand", addTilesBrand.Text }
                });
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Заполнены не все обязательные поля.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (ValidateControls(tabPage17.Controls))
            {
                try
                {
                    InsertRecord("roofs", new Dictionary<string, object>()
                {
                    { "name", addRoofName.Text },
                    { "articul", addRoofArticle.Text },
                    { "width", float.Parse(addRoofWidth.Text, CultureInfo.InvariantCulture) },
                    { "length", float.Parse(addRoofLength.Text, CultureInfo.InvariantCulture) },
                    { "price", float.Parse(addRoofPrice.Text, CultureInfo.InvariantCulture)},
                    { "description", addRoofsDesc.Text},
                    { "type", addRoofType.Text },
                    { "brand", addRoofBrand.Text }
                });
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Заполнены не все обязательные поля.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button16_Click(object sender, EventArgs e)
        {
            if (ValidateControls(tabPage19.Controls))
            {
                try
                {
                    InsertRecord("bmaterials", new Dictionary<string, object>()
                {
                    { "name", addBmatName.Text },
                    { "articul", addBmatArticle.Text },
                    { "weight", float.Parse(addBmatWeight.Text, CultureInfo.InvariantCulture) },
                    { "description", addBmatsDesc.Text},
                    { "price", float.Parse(addBmatPrice.Text, CultureInfo.InvariantCulture)},
                    { "type", addBmatType.Text },
                    { "brand", addBmatBrand.Text }
                });
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Заполнены не все обязательные поля.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            if (ValidateControls(tabPage22.Controls))
            {
                try
                {
                    InsertRecord("paints", new Dictionary<string, object>()
                {
                    { "name", paintaddName.Text },
                    { "articul", paintaddArticle.Text },
                    { "washable", paintaddWashable.Text },
                    { "price", float.Parse(paintaddPrice.Text, CultureInfo.InvariantCulture)},
                    { "description", addPaintsDesc.Text },
                    { "type", paintaddType.Text },
                    { "brand", paintaddBrand.Text }
                });
                    UpdateData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Заполнены не все обязательные поля.", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string queryString = "SELECT * FROM orders WHERE";
            if (!string.IsNullOrEmpty(orderFilterDateFrom.Text))
            {
                queryString += $" DATETIME(STRFTIME('%Y-%m-%d %H:%M:%S', datetime)) >= {orderFilterDateFrom.Text}";
            }

            if (!string.IsNullOrEmpty(orderFilterDateTo.Text))
            {
                if (!string.IsNullOrEmpty(queryString))
                {
                    queryString += " AND";
                }
                queryString += $" DATETIME(STRFTIME('%Y-%m-%d %H:%M:%S', datetime)) <= {orderFilterDateFrom.Text}";
            }

            if (!string.IsNullOrEmpty(itemCombo2.Text))
            {
                if (!string.IsNullOrEmpty(queryString))
                {
                    queryString += " AND";
                }
                queryString += $" what LIKE '%{itemCombo2.Text}%'";
            }
            else
            {
                MessageBox.Show("Не выбрано ни одно условие поиска!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void categoryCombo1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            subcategoryCombo1.Items.Clear();
            subcategoryCombo1.Items.AddRange(GetSubtypes(categoryCombo1.Text));

            if (tables.ContainsKey(categoryCombo1.Text) && (tables[categoryCombo1.Text] == "floors" || tables[categoryCombo1.Text] == "tiles"))
            {
                quantityCombo.Enabled = true;
            }
            else { quantityCombo.Enabled = false; }
        }

        private void subcategoryCombo1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var temp = new DataTable();
            var command = new SqliteCommand($"SELECT * FROM {tables[categoryCombo1.Text]} WHERE type='{subcategoryCombo1.Text}'", _connection);
            temp.Load(command.ExecuteReader());

            itemCombo1.ValueMember = "name";
            itemCombo1.DisplayMember = "name";
            itemCombo1.DataSource = temp;
        }

        private void addOrderIsDelivery_CheckedChanged(object sender, EventArgs e)
        {
            addOrderAddr.Visible = !addOrderAddr.Visible;
            addOrderAddr.Enabled = !addOrderAddr.Enabled;
            label53.Visible = !label53.Visible;
        }

        private void button20_Click(object sender, EventArgs e)
        {
            if (ValidateControl(addUserAccessLevel) && ValidateControl(addUserPass) && ValidateControl(addUserUsername) && ValidateControl(addUserPosition))
            {
                try {
                    InsertRecord("users", new Dictionary<string, object>()
                {
                    { "username", addUserUsername.Text },
                    { "position", addUserPosition.Text },
                    { "access_level", addUserAccessLevel.Text },
                    { "pass_hash", Encoder.ComputeHash(addUserPass.Text, "SHA512", null)}
                });
                    UpdateData();
                } catch (Exception ex) {
                    MessageBox.Show("Ошибка: " + ex.Message.ToString(), "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else {
                MessageBox.Show("Заполнены не все поля!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
