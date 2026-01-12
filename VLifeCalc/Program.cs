namespace VLifeCalc
{
    using System;
    using System.Text.Json;

    namespace VLifeCalc
    {
        public class Stats
        {
            public int 根気 { get; set; }
            public int 感性 { get; set; }
            public int 知性 { get; set; }
            public int 狂気 { get; set; }
            public int 色気 { get; set; }
        }

        public class Chara
        {
            public int no { get; set; }
            public string name { get; set; }
            public Stats stats { get; set; }
            public string? genre1 { get; set; }
            public string? genre2 { get; set; }
            public string? topic1 { get; set; }
            public string? topic2 { get; set; }
            public string? topic3 { get; set; }
        }

        public class Genre
        {
            public string genre { get; set; }
            public List<string> topics { get; set; }
        }

        public class Topic
        {
            public string topic { get; set; }
            public string power { get; set; }
            public List<string> topics { get; set; }

            public double powerRate
            {
                get
                {
                    return power switch
                    {
                        "やや低い" => 1.0,
                        "普通" => 1.05,
                        "やや高い" => 1.10,
                        "高い" => 1.15,
                        "とても高い" => 1.20,
                        _ => 1.0,
                    };
                }
            }
        }

        public class Result
        {
            public string result { get; set; }
            public double qualityRate { get; set; }
        }

        internal class Program
        {
            static void Main(string[] args)
            {
                var charaList = LoadJson<List<Chara>>("data/chara.json");
                var genreList = LoadJson<List<Genre>>("data/genre.json");
                var topicList = LoadJson<List<Topic>>("data/topic.json");

                // 標準入力からジャンルとトピックを受け取る
                // トレンドのジャンルを genreList に含まれていない、かつ、空でない場合は繰り返し入力を求める
                string? fixGenre;
                while (true)
                {
                    Console.Write("固定ジャンルを入力してください: ");
                    fixGenre = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(fixGenre) ||
                        genreList.Any(g => g.genre == fixGenre))
                    {
                        break;
                    }
                    Console.WriteLine("ジャンルが存在しません。もう一度入力してください。");
                }

                // トレンドのジャンルを genreList に含まれていない、かつ、空でない場合は繰り返し入力を求める
                string? trendGenreInput = null;
                while (string.IsNullOrWhiteSpace(fixGenre))
                {
                    Console.Write("トレンドのジャンルを入力してください: ");
                    trendGenreInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(trendGenreInput) ||
                        genreList.Any(g => g.genre == trendGenreInput))
                    {
                        break;
                    }
                    Console.WriteLine("ジャンルが存在しません。もう一度入力してください。");
                }


                // トレンドのネタを topicList に含まれていない、かつ、空でない場合は繰り返し入力を求める
                string? trendTopicInput;
                while (true)
                {
                    Console.Write("トレンドのネタを入力してください: ");
                    trendTopicInput = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(trendTopicInput) ||
                        topicList.Any(t => t.topic == trendTopicInput))
                    {
                        break;
                    }
                    Console.WriteLine("ネタが存在しません。もう一度入力してください。");
                }

                // トレンドのネタを topicList に含まれていない、かつ、空でない場合は繰り返し入力を求める
                List<(string genreTrend, string topicTrend, int index)> trendList = new();
                Console.Write("全トレンドを考慮に入れる場合何か入力: ");
                string? calcTrendInput = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(calcTrendInput))
                {
                    trendList.Add((trendGenreInput ?? "", trendTopicInput ?? "", 0));
                }
                else
                {
                    for (int i = 0; i < 31; i++)
                    {
                        int genreIndex = i % genreList.Count;
                        int topicIndex = i % topicList.Count;
                        trendList.Add((genreList[genreIndex].genre, topicList[topicIndex].topic, i+1));
                    }
                }


                double maxQualityRate = 0;
                List<Result> maxDatas = new();
                List<Result> allDatas = new();
                List<string> temp = new();
                foreach (var chara in charaList ?? Enumerable.Empty<Chara>())
                {
                    double charaMaxQualityRate = 0;
                    List<Result> charaMaxDatas = new();
                    foreach (var ganre in genreList ?? Enumerable.Empty<Genre>())
                    {
                        if (!string.IsNullOrWhiteSpace(fixGenre) && ganre.genre != fixGenre)
                        {
                            continue;
                        }
                        for (int i = 0; i < (topicList ?? Enumerable.Empty<Topic>()).Count(); i++)
                        {
                            Topic topic1 = (topicList ?? Enumerable.Empty<Topic>()).ElementAt(i);
                            for (int j = i; j < (topicList ?? Enumerable.Empty<Topic>()).Count(); j++)
                            {
                                Topic topic2 = (topicList ?? Enumerable.Empty<Topic>()).ElementAt(j);
                                if (topic1 == topic2)
                                {
                                    continue;
                                }
                                foreach (var (trendGenre, trendTopic, day) in trendList)
                                {
                                    // トレンドのジャンルとトピックを設定
                                    // ループ内で設定することで、全ての組み合わせを試す

                                    int modify = 10; // 新鮮値ボーナス
                                    if (chara.genre1 == ganre.genre)
                                    {
                                        modify += 15;
                                    }
                                    if (chara.genre2 == ganre.genre)
                                    {
                                        modify += 10;
                                    }
                                    if (chara.topic1 == topic1.topic || chara.topic1 == topic2.topic)
                                    {
                                        modify += 15;
                                    }
                                    if (chara.topic2 == topic1.topic || chara.topic2 == topic2.topic)
                                    {
                                        modify += 15;
                                    }
                                    if (chara.topic3 == topic1.topic || chara.topic3 == topic2.topic)
                                    {
                                        modify += 10;
                                    }
                                    if (ganre.topics.Contains(topic1.topic))
                                    {
                                        modify += 5;
                                    }
                                    if (ganre.topics.Contains(topic2.topic))
                                    {
                                        modify += 5;
                                    }
                                    if (topic1.topics.Contains(topic2.topic))
                                    {
                                        modify += 5;
                                    }
                                    if (trendGenre == ganre.genre)
                                    {
                                        modify += 15;
                                    }
                                    if (trendTopic == topic1.topic || trendTopic == topic2.topic)
                                    {
                                        modify += 10;
                                    }

                                    double qualityRate = (0.2 + (0.4 * topic1.powerRate) + (0.4 * topic2.powerRate)) * ((100 + modify) / 100.0);



                                    if (charaMaxQualityRate < qualityRate)
                                    {
                                        charaMaxQualityRate = qualityRate;
                                        charaMaxDatas.Clear();
                                    }
                                    if (charaMaxQualityRate == qualityRate)
                                    {
                                        string charaMaxData = $"レート: {qualityRate}, キャラ: {chara.name}, ジャンル: {ganre.genre}, トピック1: {topic1.topic}({topic1.power}), トピック2: {topic2.topic}({topic2.power}), トレンド: {trendGenre}-{trendTopic}({day}), 修正値(新鮮値ボーナス込み): {modify}";
                                        Result result = new Result
                                        {
                                            result = charaMaxData,
                                            qualityRate = qualityRate
                                        };

                                        charaMaxDatas.Add(result);
                                    }
                                }
                            }
                        }
                    }
                    
                    if (maxQualityRate < charaMaxQualityRate)
                    {
                        maxQualityRate = charaMaxQualityRate;
                        maxDatas.Clear();
                    }
                    if (maxQualityRate == charaMaxQualityRate)
                    {
                        maxDatas.AddRange(charaMaxDatas);
                    }
                    allDatas.AddRange(charaMaxDatas);
                }
                Console.WriteLine("===== キャラ別最大 =====");
                allDatas.Sort((a, b) => b.qualityRate.CompareTo(a.qualityRate));
                foreach (var data in allDatas)
                {
                    Console.WriteLine(data.result);
                }
                Console.WriteLine("===== 全体最大 =====");
                foreach (var maxData in maxDatas)
                {
                    Console.WriteLine(maxData.result);
                }

                Console.WriteLine($"キャラ数: {charaList?.Count}");
                Console.WriteLine($"ジャンル数: {genreList?.Count}");
                Console.WriteLine($"トピック数: {topicList?.Count}");
            }

            static T LoadJson<T>(string path)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException($"{path} が見つかりません。");
                }
                var json = File.ReadAllText(path);
                var result = JsonSerializer.Deserialize<T>(json);
                if (result == null)
                {
                    throw new InvalidOperationException($"{path} のデシリアライズに失敗しました。");
                }
                return result;
            }
        }
    }
}
