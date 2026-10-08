using BokarevChistyakov6Pr.Forms;
using BokarevChistyakov6Pr.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BokarevChistyakov6Pr
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            pictureBoxCaptch1.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxCaptch2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxCaptch3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxCaptch4.SizeMode = PictureBoxSizeMode.Zoom;
            panelCaptch.Visible = false;
            // Привязываем кнопку к обработчику входа без повторной подписки
            buttonEnter.Click -= buttonEnter_Click;
            buttonEnter.Click += buttonEnter_Click;
        }

        // Порядок изображений капчи
        private List<int> pictures = new List<int>() { 3, 4, 1, 2 };

        // Подключение к базе данных через Entity Framework
        private ModelEF modelEF = new ModelEF();

        // Аккаунт пользователя, который проходит авторизацию
        private Accounts accounts;

        // Кнопка "Войти"
        private void buttonEnter_Click(object sender, EventArgs e)
        {
            // Ищем аккаунт с указанными логином и паролем
            accounts = modelEF.Accounts.FirstOrDefault(
                x => x.Login == textBoxLogin.Text
                && x.Password == textBoxPassword.Text);

            if (accounts != null)
            {
                // Активному пользователю показываем капчу
                if (accounts.StatusID == 1)
                {
                    panelCaptch.Visible = true;
                }
                else
                {
                    MessageBox.Show(
                        "Вы заблокированы. Обратитесь к администратору");
                }

                return;
            }
            else
            {
                // Проверяем, существует ли аккаунт с таким логином
                accounts = modelEF.Accounts.FirstOrDefault(
                    x => x.Login == textBoxLogin.Text);

                if (accounts != null)
                {
                    if (accounts.StatusID == 2)
                    {
                        MessageBox.Show(
                            "Вы заблокированы. Обратитесь к администратору");
                        return;
                    }

                    // Записываем неудачную попытку входа
                    accounts.BadLoginTry += 1;
                    modelEF.SaveChanges();

                    MessageBox.Show(
                        "Вы не правильно ввели пароль. У вас осталось попыток "
                        + (3 - accounts.BadLoginTry));

                    // После трёх попыток блокируем аккаунт
                    if (accounts.BadLoginTry == 3)
                    {
                        accounts.StatusID = 2;
                        modelEF.SaveChanges();

                        MessageBox.Show(
                            "Вы заблокированы. Обратитесь к администратору");
                    }
                }
                else
                {
                    MessageBox.Show(
                        "Вы ввели неверный логин или пароль. " +
                        "Пожалуйста проверьте ещё раз введенные данные");
                }
            }
        }

        // Загружаем изображения в элементы капчи
        private void LoadPictures()
        {
            pictureBoxCaptch1.Image =
                Image.FromFile($@"Pictures\{pictures[0]}.png");

            pictureBoxCaptch2.Image =
                Image.FromFile($@"Pictures\{pictures[1]}.png");

            pictureBoxCaptch3.Image =
                Image.FromFile($@"Pictures\{pictures[2]}.png");

            pictureBoxCaptch4.Image =
                Image.FromFile($@"Pictures\{pictures[3]}.png");
        }

        // Загрузка главной формы
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadPictures();
        }

        // Кнопка "Готово" — проверка капчи
        private void buttonReady_Click(object sender, EventArgs e)
        {
            if (accounts == null)
            {
                panelCaptch.Visible = false;
                MessageBox.Show("Сначала введите логин и пароль и нажмите «Войти».");
                return;
            }
            // Капча верна, если первым стоит изображение № 1
            if (pictures[0] == 1)
            {
                panelCaptch.Visible = false;

                // Открываем форму в зависимости от роли пользователя
                if (accounts.RoleID == 1)
                {
                    AdminForm form = new AdminForm();
                    form.Show();
                    Hide();
                }
                else if (accounts.RoleID == 2)
                {
                    UserForm form = new UserForm();
                    form.Show();
                    Hide();
                }

                // После успешного входа сбрасываем количество ошибок
                accounts.BadLoginTry = 0;
                modelEF.SaveChanges();

                MessageBox.Show("Вы успешно авторизовались");
            }
            else
            {
                accounts.BadLoginTry += 1;
                modelEF.SaveChanges();

                MessageBox.Show(
                    $"Каптча не правильная осталось попыток {3 - accounts.BadLoginTry}");

                // Блокируем аккаунт после трёх неудачных попыток
                if (accounts.BadLoginTry >= 3)
                {
                    accounts.StatusID = 2;
                    modelEF.SaveChanges();

                    MessageBox.Show(
                        "Вы заблокированы. Обратитесь к администратору");

                    panelCaptch.Visible = false;
                }
            }
        }

        // Кнопка "Вправо" — меняем порядок изображений
        private void buttonNext_Click(object sender, EventArgs e)
        {
            int first = pictures[0];

            pictures[0] = pictures[1];
            pictures[1] = pictures[2];
            pictures[2] = pictures[3];

            pictures[3] = first;

            LoadPictures();
        }
    }
}