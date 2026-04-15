using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace CineMatch
{
    public class FormMain : Form
    {
        private readonly User _user;

        private TabControl tabMain;
        private TabPage tabDiscover, tabLibrary, tabProfile;

        private Label lblStatsMovies, lblStatsRatings, lblStatsUsers, lblAgeGroup;
        private Button btnGetRecs;
        private FlowLayoutPanel flpRecs;

        private TextBox txtSearch;
        private ComboBox cboGenreFilter, cboMoodFilter, cboSort;
        private FlowLayoutPanel flpLibrary;
        private Label lblLibInfo;

        private Label lblPName, lblPSince, lblPAgeGroup;
        private Label lblPWatched, lblPRated, lblPAvg, lblPAge;
        private Panel pnlGenreBars;
        private FlowLayoutPanel flpRatings;

        private Form _toastForm;

        public FormMain(User user)
        {
            _user = user;
            BuildUI();
            Shown += FormMain_Shown;
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            LoadStats();
            LoadLibrary();
        }

        private void BuildUI()
        {
            Text = "CineMatch — " + _user.FullName;
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(9, 9, 15);
            ForeColor = Color.FromArgb(238, 232, 213);

            //Nav bar
            var navBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 52,
                BackColor = Color.FromArgb(9, 9, 15),
            };

            var lblLogo = new Label
            {
                Text = "CineMatch",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(232, 197, 71),
                AutoSize = true,
                Location = new Point(16, 12),
            };
            navBar.Controls.Add(lblLogo);

            var lblUser = new Label
            {
                Text = "  " + _user.AvatarInit + "  " + _user.Username + "  [" + _user.AgeGroup + "]",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(180, 180, 180),
                AutoSize = true,
                Location = new Point(820, 16),
            };
            navBar.Controls.Add(lblUser);

            var btnLogout = new Button
            {
                Text = "Sign Out",
                Location = new Point(1000, 12),
                Size = new Size(80, 28),
                BackColor = Color.FromArgb(25, 25, 36),
                ForeColor = Color.FromArgb(232, 82, 82),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8),
                Cursor = Cursors.Hand,
            };
            btnLogout.Click += (s, e) => { new FormLogin().Show(); Close(); };
            navBar.Controls.Add(btnLogout);

            navBar.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(new Pen(Color.FromArgb(40, 232, 197, 71)),
                    0, navBar.Height - 1, navBar.Width, navBar.Height - 1);
            };
            Controls.Add(navBar);

            tabMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 10),
                Appearance = TabAppearance.FlatButtons,
            };

            tabDiscover = new TabPage("  Discover  ") { BackColor = Color.FromArgb(9, 9, 15) };
            tabLibrary = new TabPage("  Library   ") { BackColor = Color.FromArgb(9, 9, 15) };
            tabProfile = new TabPage("  My Profile") { BackColor = Color.FromArgb(9, 9, 15) };

            tabMain.TabPages.AddRange(new[] { tabDiscover, tabLibrary, tabProfile });
            tabMain.SelectedIndexChanged += (s, e) =>
            {
                if (tabMain.SelectedTab == tabProfile) LoadProfile();
            };
            Controls.Add(tabMain);

            BuildDiscoverTab();
            BuildLibraryTab();
            BuildProfileTab();
        }

  
        private void BuildDiscoverTab()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(9, 9, 15) };

            var lblHero = new Label
            {
                Text = "Your Perfect Film Awaits",
                Font = new Font("Segoe UI", 22, FontStyle.Bold),
                ForeColor = Color.FromArgb(238, 232, 213),
                AutoSize = true,
                Location = new Point(24, 24),
            };
            scroll.Controls.Add(lblHero);

            var lblTag = new Label
            {
                Text = "// 5-factor hybrid engine: genre · rating · age group · trending · watch history",
                Font = new Font("Courier New", 8),
                ForeColor = Color.FromArgb(232, 197, 71),
                AutoSize = true,
                Location = new Point(24, 60),
            };
            scroll.Controls.Add(lblTag);

            var statsPanel = new TableLayoutPanel
            {
                Location = new Point(24, 88),
                Size = new Size(700, 68),
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.FromArgb(17, 17, 25),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
            };
            for (int i = 0; i < 4; i++)
                statsPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            lblStatsMovies = MakeStatLabel("--", "Films in DB");
            lblStatsRatings = MakeStatLabel("--", "Ratings");
            lblStatsUsers = MakeStatLabel("--", "Users");
            lblAgeGroup = MakeStatLabel(_user.AgeGroup, "My Age Group");

            statsPanel.Controls.Add(lblStatsMovies, 0, 0);
            statsPanel.Controls.Add(lblStatsRatings, 1, 0);
            statsPanel.Controls.Add(lblStatsUsers, 2, 0);
            statsPanel.Controls.Add(lblAgeGroup, 3, 0);
            scroll.Controls.Add(statsPanel);

            btnGetRecs = new Button
            {
                Text = "  Get My Recommendations",
                Location = new Point(24, 172),
                Size = new Size(280, 44),
                BackColor = Color.FromArgb(232, 197, 71),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Cursor = Cursors.Hand,
            };
            btnGetRecs.Click += BtnGetRecs_Click;
            scroll.Controls.Add(btnGetRecs);

            flpRecs = new FlowLayoutPanel
            {
                Location = new Point(24, 228),
                Size = new Size(1040, 1200),
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(9, 9, 15),
            };
            scroll.Controls.Add(flpRecs);

            tabDiscover.Controls.Add(scroll);
        }

        private void BtnGetRecs_Click(object sender, EventArgs e)
        {
            btnGetRecs.Enabled = false;
            btnGetRecs.Text = "  Loading...";
            flpRecs.Controls.Clear();

            try
            {
                var sections = DataService.GetRecommendations(_user.UserId);

                if (sections.Count == 0)
                {
                    flpRecs.Controls.Add(new Label
                    {
                        Text = "Rate more movies in the Library to unlock recommendations!",
                        ForeColor = Color.FromArgb(180, 180, 180),
                        Font = new Font("Segoe UI", 10),
                        AutoSize = true,
                    });
                    return;
                }

                var titles = new Dictionary<string, string>
                {
                    { "genre",    "Based on Your Favourite Genre"    },
                    { "rating",   "Top Rated — You Haven't Seen Yet" },
                    { "trending", "Trending Now"                     },
                };

                foreach (var section in sections)
                {
                    string type = section.Type;
                    var movies = section.Movies;

                    var lbl = new Label
                    {
                        Text = titles.ContainsKey(type) ? titles[type] : type,
                        Font = new Font("Segoe UI", 13, FontStyle.Bold),
                        ForeColor = Color.FromArgb(238, 232, 213),
                        AutoSize = true,
                        Margin = new Padding(0, 16, 0, 8),
                    };
                    flpRecs.Controls.Add(lbl);

                    var row = new FlowLayoutPanel
                    {
                        FlowDirection = FlowDirection.LeftToRight,
                        WrapContents = false,
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowAndShrink,
                        BackColor = Color.FromArgb(9, 9, 15),
                        Margin = new Padding(0, 0, 0, 8),
                    };
                    foreach (var m in movies)
                        row.Controls.Add(MakeMovieCard(m));
                    flpRecs.Controls.Add(row);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "CineMatch",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnGetRecs.Enabled = true;
                btnGetRecs.Text = "  Get My Recommendations";
            }
        }

        private void LoadStats()
        {
            try
            {
                var stats = DataService.GetStats();
                lblStatsMovies.Text = FormatStat(stats.Movies.ToString(), "Films in DB");
                lblStatsRatings.Text = FormatStat(stats.Ratings.ToString(), "Ratings");
                lblStatsUsers.Text = FormatStat(stats.Users.ToString(), "Users");
            }
            catch { }
        }

        private void BuildLibraryTab()
        {
            var filterBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 50,
                BackColor = Color.FromArgb(17, 17, 25),
                Padding = new Padding(8, 6, 8, 6),
            };

            txtSearch = new TextBox
            {
                PlaceholderText = "Search films...",
                Location = new Point(8, 10),
                Size = new Size(220, 28),
                BackColor = Color.FromArgb(25, 25, 36),
                ForeColor = Color.FromArgb(238, 232, 213),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 10),
            };
            txtSearch.TextChanged += (s, e) => LoadLibrary();
            filterBar.Controls.Add(txtSearch);

            cboGenreFilter = MakeFilterCombo(new Point(240, 10), 150,
                new[] { "All Genres","Drama","Action","Comedy","Horror","Sci-Fi",
                        "Thriller","Romance","Animation","Crime","Fantasy","Biography","Musical" });
            cboGenreFilter.SelectedIndexChanged += (s, e) => LoadLibrary();
            filterBar.Controls.Add(cboGenreFilter);

            cboMoodFilter = MakeFilterCombo(new Point(400, 10), 150,
                new[] { "All Moods","Dark","Uplifting","Tense","Romantic",
                        "Funny","Mind-bending","Inspiring","Nostalgic" });
            cboMoodFilter.SelectedIndexChanged += (s, e) => LoadLibrary();
            filterBar.Controls.Add(cboMoodFilter);

            cboSort = MakeFilterCombo(new Point(560, 10), 140,
                new[] { "Top Rated", "Newest", "A-Z", "Trending" });
            cboSort.SelectedIndexChanged += (s, e) => LoadLibrary();
            filterBar.Controls.Add(cboSort);

            lblLibInfo = new Label
            {
                Location = new Point(720, 14),
                Size = new Size(320, 20),
                ForeColor = Color.FromArgb(120, 120, 140),
                Font = new Font("Courier New", 7),
            };
            filterBar.Controls.Add(lblLibInfo);

            tabLibrary.Controls.Add(filterBar);

            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(9, 9, 15) };
            flpLibrary = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = Color.FromArgb(9, 9, 15),
                Padding = new Padding(12),
            };
            scroll.Controls.Add(flpLibrary);
            tabLibrary.Controls.Add(scroll);
        }

        private void LoadLibrary()
        {
            string search = txtSearch?.Text ?? "";
            string genre = cboGenreFilter?.SelectedItem?.ToString() ?? "";
            string mood = cboMoodFilter?.SelectedItem?.ToString() ?? "";
            string sort = cboSort?.SelectedItem?.ToString() ?? "Top Rated";

            if (genre == "All Genres") genre = "";
            if (mood == "All Moods") mood = "";

            string sortKey = sort == "Newest" ? "year"
                           : sort == "A-Z" ? "title"
                           : sort == "Trending" ? "trending"
                           : "rating_avg";

            try
            {
                var movies = DataService.GetMovies(search, genre, mood, sortKey, _user.UserId);
                flpLibrary.Controls.Clear();
                foreach (var m in movies)
                    flpLibrary.Controls.Add(MakeMovieCard(m));

                if (lblLibInfo != null)
                    lblLibInfo.Text = movies.Count + " films · genre: \"" +
                                      (genre == "" ? "any" : genre) + "\" · sort: " + sortKey;
            }
            catch (Exception ex)
            {
                flpLibrary.Controls.Clear();
                flpLibrary.Controls.Add(new Label
                {
                    Text = "DB Error: " + ex.Message,
                    ForeColor = Color.FromArgb(232, 82, 82),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9),
                });
            }
        }

     
        private void BuildProfileTab()
        {
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, BackColor = Color.FromArgb(9, 9, 15) };

            var sidebar = new Panel
            {
                Location = new Point(16, 16),
                Size = new Size(260, 600),
                BackColor = Color.FromArgb(17, 17, 25),
            };
            sidebar.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(40, 232, 197, 71)),
                    0, 0, sidebar.Width - 1, sidebar.Height - 1);
            };

            var avatarPanel = new Panel { Location = new Point(90, 16), Size = new Size(80, 80) };
            avatarPanel.Paint += (s, e) =>
            {
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(232, 197, 71)), 0, 0, 79, 79);
                var fmt = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(_user.AvatarInit,
                    new Font("Segoe UI", 22, FontStyle.Bold),
                    Brushes.Black,
                    new RectangleF(0, 0, 79, 79), fmt);
            };
            sidebar.Controls.Add(avatarPanel);

            lblPName = new Label
            {
                Location = new Point(0, 106),
                Size = new Size(260, 28),
                ForeColor = Color.FromArgb(238, 232, 213),
                Font = new Font("Segoe UI", 13, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = _user.FullName,
            };
            sidebar.Controls.Add(lblPName);

            lblPSince = new Label
            {
                Location = new Point(0, 136),
                Size = new Size(260, 20),
                ForeColor = Color.FromArgb(100, 100, 130),
                Font = new Font("Courier New", 8),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            sidebar.Controls.Add(lblPSince);

            lblPAgeGroup = new Label
            {
                Location = new Point(20, 162),
                Size = new Size(220, 24),
                ForeColor = Color.FromArgb(82, 160, 232),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            sidebar.Controls.Add(lblPAgeGroup);

            var statGrid = new TableLayoutPanel
            {
                Location = new Point(10, 196),
                Size = new Size(240, 80),
                ColumnCount = 2,
                RowCount = 2,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                BackColor = Color.FromArgb(17, 17, 25),
            };
            statGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            statGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));

            lblPWatched = MakeStatLabel("0", "Watched");
            lblPRated = MakeStatLabel("0", "Rated");
            lblPAvg = MakeStatLabel("--", "Avg");
            lblPAge = MakeStatLabel("--", "Age");
            statGrid.Controls.Add(lblPWatched, 0, 0);
            statGrid.Controls.Add(lblPRated, 1, 0);
            statGrid.Controls.Add(lblPAvg, 0, 1);
            statGrid.Controls.Add(lblPAge, 1, 1);
            sidebar.Controls.Add(statGrid);

            var lblGenreTitle = new Label
            {
                Text = "GENRE TASTE MAP",
                Location = new Point(10, 286),
                Size = new Size(240, 18),
                Font = new Font("Courier New", 7),
                ForeColor = Color.FromArgb(100, 100, 130),
            };
            sidebar.Controls.Add(lblGenreTitle);

            pnlGenreBars = new Panel
            {
                Location = new Point(10, 308),
                Size = new Size(240, 280),
                BackColor = Color.Transparent,
            };
            sidebar.Controls.Add(pnlGenreBars);
            scroll.Controls.Add(sidebar);

            var mainArea = new Panel
            {
                Location = new Point(290, 16),
                Size = new Size(760, 600),
                BackColor = Color.FromArgb(9, 9, 15),
                AutoScroll = true,
            };

            var lblHistTitle = new Label
            {
                Text = "MY RATINGS  (JOIN Ratings x Movies)",
                Font = new Font("Courier New", 8),
                ForeColor = Color.FromArgb(100, 100, 130),
                Location = new Point(0, 0),
                AutoSize = true,
            };
            mainArea.Controls.Add(lblHistTitle);

            flpRatings = new FlowLayoutPanel
            {
                Location = new Point(0, 24),
                Size = new Size(750, 560),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoSize = true,
                BackColor = Color.FromArgb(9, 9, 15),
            };
            mainArea.Controls.Add(flpRatings);

            scroll.Controls.Add(mainArea);
            tabProfile.Controls.Add(scroll);
        }

        private void LoadProfile()
        {
            try
            {
                var p = DataService.GetProfile(_user.UserId);
                if (p.Info == null) return;

                lblPName.Text = p.Info.FullName;
                lblPSince.Text = "Member since " + p.Info.JoinedAt.ToString("MMM yyyy");
                lblPAgeGroup.Text = "[" + p.Info.AgeGroup + "]  DOB: " + p.Info.Dob.ToString("dd MMM yyyy");
                lblPWatched.Text = FormatStat(p.TotalRatings.ToString(), "Watched");
                lblPRated.Text = FormatStat(p.TotalRatings.ToString(), "Rated");
                lblPAvg.Text = FormatStat(p.AvgStars > 0 ? p.AvgStars.ToString("F1") : "--", "Avg");
                lblPAge.Text = FormatStat(p.Info.Age.ToString(), "Age");

                pnlGenreBars.Controls.Clear();
                int by = 0;
                foreach (var gp in p.GenrePrefs)
                {
                    var rowPanel = new Panel
                    {
                        Location = new Point(0, by),
                        Size = new Size(230, 26),
                        BackColor = Color.Transparent
                    };

                    var genreLabel = new Label
                    {
                        Text = gp.Genre,
                        Location = new Point(0, 5),
                        Size = new Size(100, 18),
                        Font = new Font("Segoe UI", 9),
                        ForeColor = Color.FromArgb(200, 200, 200),
                    };

                    int barWidth = (int)(gp.Weight * 100);
                    var barBg = new Panel { Location = new Point(106, 8), Size = new Size(100, 6), BackColor = Color.FromArgb(30, 30, 45) };
                    var barFill = new Panel { Location = new Point(0, 0), Size = new Size(barWidth, 6), BackColor = Color.FromArgb(232, 197, 71) };
                    barBg.Controls.Add(barFill);

                    rowPanel.Controls.Add(genreLabel);
                    rowPanel.Controls.Add(barBg);
                    pnlGenreBars.Controls.Add(rowPanel);
                    by += 30;
                }

                flpRatings.Controls.Clear();
                if (p.Ratings.Count == 0)
                {
                    flpRatings.Controls.Add(new Label
                    {
                        Text = "// No ratings yet — head to the Library!",
                        ForeColor = Color.FromArgb(120, 120, 140),
                        Font = new Font("Courier New", 9),
                        AutoSize = true,
                        Margin = new Padding(0, 12, 0, 0),
                    });
                }
                else
                {
                    foreach (var r in p.Ratings)
                        flpRatings.Controls.Add(MakeRatingRow(r));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Profile error: " + ex.Message, "CineMatch",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        
        private Panel MakeMovieCard(Movie m)
        {
            var card = new Panel
            {
                Size = new Size(200, 290),
                BackColor = Color.FromArgb(17, 17, 25),
                Margin = new Padding(6),
            };
            card.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(40, 232, 197, 71)),
                    0, 0, card.Width - 1, card.Height - 1);
            };

            var genreColors = new Dictionary<string, Color>
            {
                { "Action",    Color.FromArgb(26, 15, 15) },
                { "Drama",     Color.FromArgb(26, 20, 32) },
                { "Comedy",    Color.FromArgb(21, 26, 15) },
                { "Horror",    Color.FromArgb(18, 16, 26) },
                { "Sci-Fi",    Color.FromArgb(15, 21, 32) },
                { "Thriller",  Color.FromArgb(15, 26, 20) },
                { "Romance",   Color.FromArgb(26, 15, 18) },
                { "Animation", Color.FromArgb(26, 21, 15) },
                { "Fantasy",   Color.FromArgb(19, 15, 26) },
                { "Crime",     Color.FromArgb(26, 16, 15) },
                { "Biography", Color.FromArgb(26, 26, 15) },
                { "Musical",   Color.FromArgb(26, 15, 24) },
            };

            string pg = m.PrimaryGenre;

            var poster = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(200, 100),
                BackColor = genreColors.ContainsKey(pg) ? genreColors[pg] : Color.FromArgb(17, 17, 25),
            };
            poster.Paint += (s, e) =>
            {
                var fmt = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(pg,
                    new Font("Segoe UI", 14, FontStyle.Bold),
                    new SolidBrush(Color.FromArgb(80, 232, 197, 71)),
                    new RectangleF(0, 0, 200, 80), fmt);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(80, 0, 0, 0)), 4, 76, 192, 20);
                e.Graphics.DrawString(pg,
                    new Font("Courier New", 7),
                    new SolidBrush(Color.FromArgb(200, 200, 200)),
                    new RectangleF(4, 78, 192, 16),
                    new StringFormat { Alignment = StringAlignment.Center });
            };
            card.Controls.Add(poster);

            var lblTitle = new Label
            {
                Text = m.Title,
                Location = new Point(8, 106),
                Size = new Size(184, 36),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(238, 232, 213),
            };
            card.Controls.Add(lblTitle);

            var lblMeta = new Label
            {
                Text = m.ReleaseYear + "  .  " + m.Director,
                Location = new Point(8, 142),
                Size = new Size(184, 16),
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.FromArgb(120, 120, 140),
            };
            card.Controls.Add(lblMeta);

            var lblRating = new Label
            {
                Text = "* " + m.RatingAvg.ToString("F1") + "  (" + m.RatingCount + ")",
                Location = new Point(8, 160),
                Size = new Size(184, 18),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                ForeColor = Color.FromArgb(232, 197, 71),
                Tag = "ratingLabel",
            };
            card.Controls.Add(lblRating);

            string desc = m.Description != null && m.Description.Length > 80
                          ? m.Description.Substring(0, 80) + "..."
                          : m.Description ?? "";
            var lblDesc = new Label
            {
                Text = desc,
                Location = new Point(8, 180),
                Size = new Size(184, 52),
                Font = new Font("Segoe UI", 7),
                ForeColor = Color.FromArgb(160, 160, 180),
            };
            card.Controls.Add(lblDesc);

            var sep = new Panel
            {
                Location = new Point(8, 234),
                Size = new Size(184, 1),
                BackColor = Color.FromArgb(40, 232, 197, 71)
            };
            card.Controls.Add(sep);

            var lblRate = new Label
            {
                Text = "RATE",
                Location = new Point(8, 242),
                Size = new Size(35, 16),
                Font = new Font("Courier New", 7),
                ForeColor = Color.FromArgb(100, 100, 130),
            };
            card.Controls.Add(lblRate);

            // Star buttons
            int currentRating = m.UserRating;
            var stars = new Button[5];

            for (int i = 0; i < 5; i++)
            {
                int capturedI = i;
                int capturedVal = i + 1;
                int capturedId = m.MovieId;

                var btn = new Button
                {
                    Text = "*",
                    Location = new Point(46 + i * 30, 238),
                    Size = new Size(26, 26),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 11),
                    BackColor = Color.Transparent,
                    ForeColor = i < currentRating
                                ? Color.FromArgb(232, 197, 71)
                                : Color.FromArgb(60, 60, 80),
                    Cursor = Cursors.Hand,
                };
                btn.FlatAppearance.BorderSize = 0;

                btn.MouseEnter += (s, e) =>
                {
                    for (int j = 0; j < 5; j++)
                        stars[j].ForeColor = j <= capturedI
                            ? Color.FromArgb(232, 197, 71)
                            : Color.FromArgb(60, 60, 80);
                };

                btn.MouseLeave += (s, e) =>
                {
                    for (int j = 0; j < 5; j++)
                        stars[j].ForeColor = j < currentRating
                            ? Color.FromArgb(232, 197, 71)
                            : Color.FromArgb(60, 60, 80);
                };

                btn.Click += (s, e) =>
                {
                    try
                    {
                        DataService.SaveRating(_user.UserId, capturedId, capturedVal);
                        currentRating = capturedVal;
                        lblRating.Text = "* Your rating: " + capturedVal + "/5";
                        for (int j = 0; j < 5; j++)
                            stars[j].ForeColor = j < capturedVal
                                ? Color.FromArgb(232, 197, 71)
                                : Color.FromArgb(60, 60, 80);
                        ShowToast("Rated \"" + m.Title + "\" " + capturedVal + " stars");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Rating error: " + ex.Message);
                    }
                };

                stars[i] = btn;
                card.Controls.Add(btn);
            }

            return card;
        }

        //  RATING HISTORY ROW
    
        private Panel MakeRatingRow(RatingHistory r)
        {
            var row = new Panel
            {
                Size = new Size(740, 56),
                BackColor = Color.FromArgb(17, 17, 25),
                Margin = new Padding(0, 0, 0, 6),
            };
            row.Paint += (s, e) =>
            {
                e.Graphics.DrawRectangle(new Pen(Color.FromArgb(30, 232, 197, 71)),
                    0, 0, row.Width - 1, row.Height - 1);
            };

            var lblTitle = new Label
            {
                Text = r.MovieTitle,
                Location = new Point(12, 8),
                Size = new Size(400, 20),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                ForeColor = Color.FromArgb(238, 232, 213),
            };

            var lblDate = new Label
            {
                Text = r.RatedAt.ToString("dd MMM yyyy"),
                Location = new Point(12, 30),
                Size = new Size(200, 16),
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(120, 120, 140),
            };

            string starStr = new string('*', r.Stars) + new string('-', 5 - r.Stars);
            var lblStars = new Label
            {
                Text = starStr,
                Location = new Point(660, 16),
                Size = new Size(70, 24),
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.FromArgb(232, 197, 71),
                TextAlign = ContentAlignment.MiddleRight,
            };

            row.Controls.Add(lblTitle);
            row.Controls.Add(lblDate);
            row.Controls.Add(lblStars);
            return row;
        }

        //  TOAST
    
        private void ShowToast(string message)
        {
            _toastForm?.Close();
            _toastForm = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(300, 52),
                BackColor = Color.FromArgb(30, 30, 45),
                ShowInTaskbar = false,
                TopMost = true,
                Opacity = 0.95,
            };

            int x = Screen.PrimaryScreen.WorkingArea.Right - 316;
            int y = Screen.PrimaryScreen.WorkingArea.Bottom - 68;
            _toastForm.Location = new Point(x, y);

            var lbl = new Label
            {
                Text = "OK  " + message,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(232, 197, 71),
                TextAlign = ContentAlignment.MiddleCenter,
            };
            _toastForm.Controls.Add(lbl);
            _toastForm.Show(this);

            var timer = new Timer { Interval = 2800 };
            timer.Tick += (s, e) => { timer.Stop(); _toastForm?.Close(); };
            timer.Start();
        }
        //  HELPERS

        private Label MakeStatLabel(string value, string caption)
        {
            return new Label
            {
                Text = FormatStat(value, caption),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 8),
                ForeColor = Color.FromArgb(200, 200, 200),
                BackColor = Color.FromArgb(17, 17, 25),
            };
        }

        private string FormatStat(string value, string caption) =>
            value + "\n" + caption.ToUpper();

        private ComboBox MakeFilterCombo(Point loc, int width, string[] items)
        {
            var c = new ComboBox
            {
                Location = loc,
                Size = new Size(width, 28),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(25, 25, 36),
                ForeColor = Color.FromArgb(238, 232, 213),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9),
            };
            c.Items.AddRange(items);
            c.SelectedIndex = 0;
            return c;
        }
    }
}