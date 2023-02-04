using AngleSharp;
using AngleSharp.Dom;
using System.Collections.Concurrent;
using System.Xml.Linq;

namespace libToGetVacancies
{

    // Класс для идентификации контакта
    public class VacancyData
    {
        public string Vacancy { get; set; }
        public Uri Link { get; set; }
        public string Date { get; set; }

        public VacancyData(string vacancy, string link, string date) {
            Vacancy = vacancy;
            Link = new System.Uri(link);
            Date = date;
        }
    }

    public class FuncToGetVacancies
    {
        public static async Task<(bool, int)> getNumberOfPages(string mainPage, 
                                                string selector = "div > div > main > div.feed-pagination.flex.align-center", 
                                                string attributeName = "data-total")
        {

            int numberOfPages;

            // проверка на возможность возврата значения
            try
            {
                IConfiguration config = Configuration.Default.WithDefaultLoader();
                IBrowsingContext context = BrowsingContext.New(config);
                IDocument document = await context.OpenAsync(mainPage);

                // Проверка успешного запроса
                if (document.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return (false, 0);
                }

                // Парсинг с проверкой результата
                if (!int.TryParse(document.QuerySelector(selector).GetAttribute(attributeName), out numberOfPages))
                {
                    return (false, 0);
                }
            }
            catch (Exception)
            {
                return (false, 0);
            }
            return (true, numberOfPages);
        }

        public static async Task<bool> fillVacanciesFromPage(ConcurrentBag<VacancyData> bag, string pageAddress)
        {
            // проверка на возможность заполнения данных
            try
            {
                IConfiguration config = Configuration.Default.WithDefaultLoader();

                IBrowsingContext context = BrowsingContext.New(config);
                IDocument document = await context.OpenAsync(pageAddress);

                // Проверка успешного запроса
                if (document.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    return false;
                }

                string selector = "div.feed__items > div > article";
                IHtmlCollection<IElement> articles = document.QuerySelectorAll(selector);

                foreach (var article in articles)
                {
                    bag.Add(new VacancyData(
                        article.QuerySelector("div > div > a > div.flex.align-between > h2").TextContent,
                        document.Origin + article.QuerySelector("div > div.preview-card__content > a").GetAttribute("href"),
                        article.QuerySelector("div > div.preview-card__publish > div.publish-info").GetAttribute("title"))
                    );
                }
            }
            catch (Exception)
            {
                return (false);
            }

            return (true);
        }
    }

}