using CsvHelper;
using System.IO;
using System.Runtime.InteropServices;

namespace CSConsoleApp
{
    public static class Program
    {
        public static void Main()
        {
            var currentDirectory = System.IO.Directory.GetCurrentDirectory();
            var filePath = System.IO.Directory.GetFiles(currentDirectory, "*.csv").First();

            IReadOnlyList<MovieCredit> movieCredits = null;
            try
            {
                var parser = new MovieCreditsParser(filePath);
                movieCredits = parser.Parse();
            }
            catch (Exception)
            {
                Console.WriteLine("Не удалось распарсить csv");
                Environment.Exit(1);
            }

            //Найти все фильмы, снятые режиссером "Steven Spielberg".

            var spielbergMovies = movieCredits
                .Where(movie => movie.Crew.Any(c =>
                    c.Job == "Director" && c.Name == "Steven Spielberg"))
                .Select(movie => movie.Title)
                .ToList();



            Console.WriteLine("\nФильмы, снятые режиссёром Steven Spielberg:");
            if (spielbergMovies.Any())
                Console.WriteLine(string.Join(Environment.NewLine, spielbergMovies));
            else
                Console.WriteLine("Фильмы не найдены.");

            //Получить список всех персонажей, которых сыграл актер "Tom Hanks".
            var tomHanksCharacters = movieCredits
                .SelectMany(movie => movie.Cast)
                .Where(castMember => castMember.Name == "Tom Hanks")
                .Select(castMember => castMember.Character)
                .Distinct()
                .ToList();



            Console.WriteLine("\nПерсонажи, которых сыграл Tom Hanks:");
            if (tomHanksCharacters.Any())
                Console.WriteLine(string.Join(Environment.NewLine, tomHanksCharacters));
            else
                Console.WriteLine("Персонажи не найдены.");

            //Найти 5 фильмов с самым большим количеством актёров.
            var top5MoviesByCastSize = movieCredits
                .Select(movie => new
                {
                    movie.Title,
                    CastCount = movie.Cast.Count
                })
                .OrderByDescending(movie => movie.CastCount)
                .Take(5)
                .ToList();


            Console.WriteLine("\nТоп 5 фильмов с самым большим количеством актёров:");
            foreach (var movie in top5MoviesByCastSize)
            {
                Console.WriteLine($"{movie.Title} — {movie.CastCount} актёров");
            }

            //Найти топ-10 самых востребованных актеров (по количеству фильмов).
            var top10Actors = movieCredits
                .SelectMany(movie => movie.Cast)
                .GroupBy(c => c.Name)
                .Select(g => new { ActorName = g.Key, MovieCount = g.Count() })
                .OrderByDescending(a => a.MovieCount)
                .Take(10)
                .ToList();


            Console.WriteLine("\nТоп-10 самых востребованных актёров:");
            foreach (var a in top10Actors)
            {
                Console.WriteLine($"{a.ActorName} — {a.MovieCount} фильмов");
            }

            //Получить список всех уникальных департаментов (department) съемочной группы.
            var uniqueDepartments = movieCredits
                .SelectMany(movie => movie.Crew)
                .Select(c => c.Department)
                .Distinct()
                .OrderBy(d => d)
                .ToList();


            Console.WriteLine("\nВсе уникальные департаменты съёмочной группы:");
            Console.WriteLine(string.Join(Environment.NewLine, uniqueDepartments));


            //Найти все фильмы, где "Hans Zimmer" был композитором (Original Music Composer).
            var hansZimmerMovies = movieCredits
                .Where(movie => movie.Crew.Any(c => c.Name == "Hans Zimmer" && c.Job == "Original Music Composer"))
                .Select(movie => movie.Title)
                .ToList();


            Console.WriteLine("\nФильмы, где Hans Zimmer был композитором:");
            Console.WriteLine(string.Join(Environment.NewLine, hansZimmerMovies));

            //Создать словарь, где ключ — ID фильма, а значение — имя режиссера.
            var directorByMovieId = movieCredits
                .Select(movie => new
                {
                    movie.MovieId,
                    Director = movie.Crew.FirstOrDefault(c => c.Job == "Director")?.Name
                })
                .Where(x => x.Director != null)
                .ToDictionary(x => x.MovieId, x => x.Director);


            Console.WriteLine("\nСловарь: ID фильма -> режиссёр");
            foreach (var kvp in directorByMovieId)
                Console.WriteLine($"{kvp.Key} → {kvp.Value}");

            //Найти фильмы, где в актерском составе есть и "Brad Pitt", и "George Clooney".
            var pittAndClooneyMovies = movieCredits
                .Where(movie => movie.Cast.Any(c => c.Name == "Brad Pitt") &&
                                movie.Cast.Any(c => c.Name == "George Clooney"))
                .Select(movie => movie.Title)
                .ToList();

            
            Console.WriteLine("\nФильмы, где снимались и Brad Pitt, и George Clooney:");
            Console.WriteLine(string.Join(Environment.NewLine, pittAndClooneyMovies));

            //Посчитать, сколько всего человек работает в департаменте "Camera" по всем фильмам.
            var totalCameraCrew = movieCredits
                .SelectMany(movie => movie.Crew)
                .Where(c => c.Department == "Camera")
                .Select(c => c.Id)
                .Distinct()
                .Count();

            
            Console.WriteLine($"\nВсего людей, работавших в департаменте Camera: {totalCameraCrew}");

            //Найти всех людей, которые в фильме "Titanic" были одновременно и в съемочной группе, и в списке актеров.
            var titanic = movieCredits.FirstOrDefault(m => m.Title == "Titanic");

            if (titanic != null)
            {
                var crewNames = titanic.Crew.Select(c => c.Name).ToHashSet();
                var castNames = titanic.Cast.Select(c => c.Name).ToHashSet();

                var bothCrewCast = crewNames.Intersect(castNames).ToList();

                
                Console.WriteLine("\nЛюди, которые были и в актёрском составе, и в съёмочной группе фильма Титаник:");
                Console.WriteLine(bothCrewCast.Any() ? string.Join(Environment.NewLine, bothCrewCast) : "Нет таких людей");
            }

            //Найти "внутренний круг" режиссера: Для режиссера "Quentin Tarantino" найти топ-5 членов съемочной группы (не актеров), которые работали с ним над наибольшим количеством фильмов.
            var tarantinoMovies = movieCredits
                .Where(m => m.Crew.Any(c => c.Job == "Director" && c.Name == "Quentin Tarantino"))
                .ToList();

            var tarantinoCrew = tarantinoMovies
                .SelectMany(m => m.Crew)
                .Where(c => c.Job != "Director")
                .GroupBy(c => c.Name)
                .Select(g => new { CrewMember = g.Key, MovieCount = g.Count() })
                .OrderByDescending(x => x.MovieCount)
                .Take(5)
                .ToList();

            
            Console.WriteLine("\nТоп-5 членов съёмочной группы, чаще всего работавших с Quentin Tarantino:");
            foreach (var member in tarantinoCrew)
                Console.WriteLine($"{member.CrewMember} — {member.MovieCount} фильмов");

            //Определить экранные "дуэты": Найти 10 пар актеров, которые чаще всего снимались вместе в одних и тех же фильмах.
            var duetCounts = movieCredits
                .SelectMany(m => m.Cast.Select(c => new { m.MovieId, c.Id, c.Name }))
                .GroupBy(x => x.MovieId)
                .SelectMany(g =>
                {
                    var list = g.ToList();
                    var pairs = new List<(string A, string B)>();
                    for (int i = 0; i < list.Count; i++)
                        for (int j = i + 1; j < list.Count; j++)
                        {
                            var a = list[i].Name;
                            var b = list[j].Name;
                            if (string.Compare(a, b) < 0)
                                pairs.Add((a, b));
                            else
                                pairs.Add((b, a));
                        }
                    return pairs;
                })
                .GroupBy(p => p)
                .Select(g => new { g.Key.A, g.Key.B, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToList();

            
            Console.WriteLine("Топ 10 дуэтов:");
            foreach (var d in duetCounts)
                Console.WriteLine($"{d.A} и {d.B} — {d.Count} фильмов");

            //Вычислить "индекс разнообразия" для карьеры: Найти 5 членов съемочной группы, которые поработали в наибольшем количестве различных департаментов за свою карьеру.
            var diversityIndex = movieCredits
                .SelectMany(m => m.Crew)
                .GroupBy(c => c.Name)
                .Select(g => new { Name = g.Key, Depts = g.Select(x => x.Department).Distinct().Count() })
                .OrderByDescending(x => x.Depts)
                .Take(5)
                .ToList();

            
            Console.WriteLine("\nСамые разнообразные:");
            foreach (var d in diversityIndex)
                Console.WriteLine($"{d.Name} — {d.Depts} департаментов");

            //Найти "творческие трио": Найти фильмы, где один и тот же человек выполнял роли режиссера(Director), сценариста(Writer) и продюсера(Producer).
            var creativeTrios = movieCredits
                .Where(m => m.Crew.GroupBy(c => c.Name)
                .Any(g => g.Select(x => x.Job).Distinct()
                .Intersect(new[] { "Director", "Writer", "Producer" }).Count() == 3))
                .Select(m => m.Title)
                .ToList();

            
            Console.WriteLine("\nФильмы, где один человек был режиссером, сценаристом и продюсером:");
            Console.WriteLine(string.Join("\n", creativeTrios));

            //Два шага до Кевина Бейкона: Найти всех актеров, которые снимались в одном фильме с актером, который, в свою очередь, снимался в одном фильме с "Kevin Bacon".
            var kevinMovies = movieCredits
                .Where(m => m.Cast.Any(c => c.Name == "Kevin Bacon"))
                .Select(m => m.MovieId)
                .ToHashSet();

            var firstLevel = movieCredits
                .Where(m => m.Cast.Any(c => kevinMovies.Contains(m.MovieId)))
                .SelectMany(m => m.Cast.Select(c => c.Name))
                .Where(n => n != "Kevin Bacon")
                .Distinct()
                .ToList();

            var secondLevel = movieCredits
                .Where(m => m.Cast.Any(c => firstLevel.Contains(c.Name)))
                .SelectMany(m => m.Cast.Select(c => c.Name))
                .Where(n => n != "Kevin Bacon" && !firstLevel.Contains(n))
                .Distinct()
                .ToList();

            
            Console.WriteLine("\nАктеры в двух шагах от Kevin Bacon:");
            Console.WriteLine(string.Join("\n", secondLevel));

            //Проанализировать "командную работу": Сгруппировать фильмы по режиссеру и для каждого из них найти средний размер как актерского состава(Cast), так и съемочной группы(Crew).
            var directorStats = movieCredits
                .SelectMany(m => m.Crew.Where(c => c.Job == "Director").Select(c => new
                {
                    c.Name,
                    CastCount = m.Cast.Count,
                    CrewCount = m.Crew.Count
                }))
                .GroupBy(x => x.Name)
                .Select(g => new
                {
                    Director = g.Key,
                    AvgCast = g.Average(x => x.CastCount),
                    AvgCrew = g.Average(x => x.CrewCount)
                })
                .ToList();

            
            Console.WriteLine("\nСредний состав по режиссерам:");
            foreach (var d in directorStats)
                Console.WriteLine($"{d.Director}: Cast {d.AvgCast:F1}, Crew {d.AvgCrew:F1}");

            //Определить карьерный путь "универсалов": Для каждого человека, который был и актером, и членом съемочной группы(в целом по датасету), определить департамент, в котором он работал чаще всего.
            var universals = movieCredits
                .SelectMany(m => m.Cast.Select(c => c.Name))
                .Intersect(movieCredits.SelectMany(m => m.Crew.Select(c => c.Name)))
                .Select(name => new
                {
                    Name = name,
                    TopDept = movieCredits.SelectMany(m => m.Crew)
                        .Where(c => c.Name == name)
                        .GroupBy(c => c.Department)
                        .OrderByDescending(g => g.Count())
                        .Select(g => g.Key)
                        .FirstOrDefault()
                })
                .ToList();

            
            Console.WriteLine("\nУниверсалы и департамент в котором они работали чаще всего:");
            foreach (var u in universals)
                Console.WriteLine($"{u.Name} — {u.TopDept}");

            //Найти пересечение "элитных клубов": Найти людей, которые работали и с режиссером "Martin Scorsese", и с режиссером "Christopher Nolan".
            var scorseseMovies = movieCredits
                .Where(m => m.Crew
                .Any(c => c.Job == "Director" && c.Name == "Martin Scorsese"))
                .Select(m => m.MovieId)
                .ToList();
            var nolanMovies = movieCredits
                .Where(m => m.Crew
                .Any(c => c.Job == "Director" && c.Name == "Christopher Nolan"))
                .Select(m => m.MovieId).ToList();

            var scorsesePeople = movieCredits
                .Where(m => scorseseMovies
                .Contains(m.MovieId))
                .SelectMany(m => m.Cast
                .Select(c => c.Name)
                .Concat(m.Crew.Select(c => c.Name)))
                .Distinct();
            var nolanPeople = movieCredits
                .Where(m => nolanMovies
                .Contains(m.MovieId))
                .SelectMany(m => m.Cast
                .Select(c => c.Name)
                .Concat(m.Crew
                .Select(c => c.Name)))
                .Distinct();

            var both = scorsesePeople
                .Intersect(nolanPeople).ToList();

            
            Console.WriteLine("\nРаботали и со Скорсезе, и с Ноланом:");
            Console.WriteLine(string.Join("\n", both));

            //Выявить "скрытое влияние": Ранжировать все департаменты по среднему количеству актеров в тех фильмах, над которыми они работали(чтобы проверить, коррелирует ли работа определенного департамента с масштабом актерского состава).
            var deptInfluence = movieCredits
                .SelectMany(m => m.Crew.Select(c => new { c.Department, CastSize = m.Cast.Count }))
                .GroupBy(x => x.Department)
                .Select(g => new { Dept = g.Key, AvgCast = g.Average(x => x.CastSize) })
                .OrderByDescending(x => x.AvgCast)
                .ToList();

            
            Console.WriteLine("\nСредний размер актерской группы по департаментам:");
            foreach (var d in deptInfluence)
                Console.WriteLine($"{d.Dept} — {d.AvgCast:F1}");

            //Проанализировать "архетипы" персонажей: Для актера "Johnny Depp" сгруппировать его роли по первому слову в имени персонажа(например, "Captain", "Jack", "Willy") и посчитать частоту каждого такого "архетипа".
            var deppRoles = movieCredits
                .SelectMany(m => m.Cast.Where(c => c.Name == "Johnny Depp").Select(c => c.Character))
                .Where(ch => !string.IsNullOrWhiteSpace(ch))
                .Select(ch => ch.Split(' ')[0])
                .GroupBy(x => x)
                .Select(g => new { Word = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            
            Console.WriteLine("\nАрхетипы Джонни Деппа:");
            foreach (var r in deppRoles)
                Console.WriteLine($"{r.Word} — {r.Count}");
        }
    }
}