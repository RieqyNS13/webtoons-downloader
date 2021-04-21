using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
namespace webtoons_download
{
	class Program
	{
		static string contentType;
		static CookieContainer cookie;
		static Stream get(string url, ref string error,Dictionary<string,string> header=null)
		{
			HttpWebRequest req;
			Stream stream;
			HttpWebResponse resp = null;
			try
			{
				req = HttpWebRequest.Create(url) as HttpWebRequest;
				req.Accept = "*/*";
				req.AllowAutoRedirect = true;
				req.UserAgent = "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36";
				if(header!= null)
				{
					foreach(KeyValuePair<string,string> pair in header)
					{
						if (pair.Key.ToLower() == "referer") req.Referer = pair.Value;
						else req.Headers.Add(pair.Key, pair.Value);
					}
				}
				req.CookieContainer = cookie;
				resp = req.GetResponse() as HttpWebResponse;
				responCode = resp.StatusCode;
				contentType = resp.ContentType;
				stream = resp.GetResponseStream();
				return stream;
			}
			catch (WebException Wex)
			{
				Console.WriteLine(Wex.ToString());
				try
				{
					resp = Wex.Response as HttpWebResponse;
					responCode = resp.StatusCode;
					contentType = null;
					return resp.GetResponseStream();
				}
				catch (Exception ex1)
				{
					responCode = 0;
					contentType = null;
					return null;
				}
			}
			catch (Exception ex)
			{

				Console.WriteLine(ex.ToString());
				error = ex.ToString();
				responCode = 0;
				contentType = null;
				return null;
			}
		}
		static HttpStatusCode responCode;
		static string getTextFromSteam(Stream stream)
		{
			try
			{
				StreamReader reader = new StreamReader(stream);
				string asu = reader.ReadToEnd();
				reader.Close();
				return asu;
			}
			catch
			{
				return null;
			}
		}
		class Gay
		{
			public Gay()
			{
				listUrl = new List<string>();
				listJudul = new List<string>();
			}
			public List<string> listUrl { get; set; }
			public List<string> listJudul { get; set; }
		}
		static Gay getListChapter(string url)
		{
			Gay gay = new Gay();
			bool next = true;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine("[+] Proses Mengumpulkan list chapter : ");
			string error = null;
			string page = "1";
			string curl = getTextFromSteam(get(url + "&page=" + page, ref error));
            while(next)
			{
				Match getpage = Regex.Match(curl, "<span class='on'>(.*?)</span>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
				page = getpage.Groups[1].Value;
				Console.WriteLine("> " + url);
				//Console.ReadKey();
				Match x = Regex.Match(curl, "<ul id=\"_listUl\">\\s*(.*?)\\s*</ul>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
				string asu = null;
				if (x.Groups.Count == 2)
				{
					asu = x.Groups[1].Value;

					MatchCollection matches= Regex.Matches(asu, "<li id=\".*?\" data-episode-no=\".*?\">\\s+<a href=\"(.*?)\"", RegexOptions.IgnoreCase);
					string[] jancuk = new string[matches.Count];
					MatchCollection matches2 = Regex.Matches(asu, "<span class=\"subj\"><span>(.*?)</span>", RegexOptions.IgnoreCase);
					if (matches.Count == matches2.Count*3)
					{
						for (int i = 0; i < matches2.Count; i++)
							jancuk[i] = matches[i * 3].Groups[1].Value;
					}
					else
					{
						for (int i = 0; i < matches.Count; i++)
							jancuk[i] = matches[i].Groups[1].Value;
					}
					for (int j=0;j < matches2.Count; j++){
						gay.listUrl.Add(jancuk[j]);
						gay.listJudul.Add(matches2[j].Groups[1].Value);
					}

				}
				Match getnextpage = Regex.Match(curl, "<span class='on'>.+?</span></a>\\s+<a href=\"(.+?)\"", RegexOptions.IgnoreCase);
				if (getnextpage.Success)
				{
					page = getnextpage.Groups[1].Value;
					Uri uri = new Uri(url);
					url = uri.Scheme + "://" + uri.Host + page;
					curl = getTextFromSteam(get(url, ref error));

				}
				else next = false;
									
			}
			Console.ForegroundColor = ConsoleColor.Green;
			string[] urls = new string[gay.listUrl.Count];
			string[] text = new string[gay.listJudul.Count];
			for(int i=0; i < gay.listUrl.Count; i++)
			{
				urls[i] = gay.listUrl[gay.listUrl.Count - 1 - i];
				text[i] = gay.listJudul[gay.listJudul.Count - 1 - i];
			}
			gay.listUrl = urls.ToList();
			gay.listJudul = text.ToList();
			return gay;
		}
		//static bool cekBedaStr(string data,string data2)
		//{
		//	try {
		//		Regex r = new Regex("<title>(.*?)</title>", RegexOptions.IgnoreCase);
		//		Match m = r.Match(data);
		//		Match m2 = r.Match(data2);
		//		if (string.Compare(m.Groups[1].Value, m2.Groups[1].Value) != 0) return true;
		//		else return false;
		//	}
		//	catch
		//	{
		//		return false;
		//	}
		//}
		static void Main(string[] args)
		{
			ServicePointManager.Expect100Continue = true;
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

			Console.Title = "Webtoons Downloader by RieqyNS13 (fb.me/rieqyns13) ";
			string url;
			Console.ForegroundColor = ConsoleColor.Green;
			
			Console.Write("[+] Masukkan url (contoh: http://www.webtoons.com/id/fantasy/noblesse/list?title_no=442 ) = ");
			url = Console.ReadLine().Trim();
			url = Regex.Replace(url, "&?page=\\d*", string.Empty);
			string error = null;
			cookie = new CookieContainer();
			string curl = getTextFromSteam(get(url, ref error));
			if (curl == null)
			{
				if (responCode == HttpStatusCode.NotFound) Console.WriteLine("[+] Not Found anjing");
				else Console.WriteLine("[+] Koneksi cacat njing "+responCode.ToString());
				Console.Read();
				return;
			}
			Gay bacod = getListChapter(url);
			Console.WriteLine("[+] Jumlah chapter : " + bacod.listUrl.Count);
			Console.ForegroundColor = ConsoleColor.Cyan;
			if (bacod.listUrl.Count == 0)
			{
				Console.WriteLine("[+] Kosong cuk");
				Console.Read();
				return;
			}
			else
			{
				Console.WriteLine();

				for (int i = 0; i < bacod.listUrl.Count; i++)
				{
					Console.WriteLine("[" + i + "] " + bacod.listJudul[i]);
					
				}
				Console.WriteLine();
			}
			Console.ForegroundColor = ConsoleColor.Green;
			string range;
			int begin = 0, end = 0;
			do
			{
				Console.Write("[+] Range chapter yang didownload (contoh: 0-11) = ");
				range = Console.ReadLine();
			} while (!cekInputRange(bacod.listUrl, range, ref begin, ref end));

			do
			{
				Console.Write("[+] Tulis folder untuk menyimpan manga (contoh: f:\\manga) = ");
				foldersimpan = Console.ReadLine().Trim();
			} while (!Directory.Exists(foldersimpan.Trim()));
			string timpa;
			do
			{
				Console.Write("[+] Timpa file yang sudah ada ? [y/n] = ");
				timpa = Console.ReadLine().Trim().ToLower();
			} while (timpa != "y" && timpa != "n");
			proses(bacod.listUrl, bacod.listJudul, begin, end, timpa == "y" ? true : false);
			Console.Read();

		}
		static string foldersimpan;
		static void downloadGambar(string url, int nomer, string pathfolder,string url_referer)
		{
			try
			{
				int i = 0; bool sukses = false; string error = null;
				while (!sukses && i < 10)
				{
					string file;
					Dictionary<string, string> header = new Dictionary<string, string>();
					header.Add("Referer", url_referer);
					Stream streamImg = get(url, ref error, header);
					if (responCode == HttpStatusCode.OK)
					{
						switch (contentType)
						{
							case "image/jpeg":
								file = pathfolder.Trim() + "/" + nomer + ".jpg";
								htmlCok += "<img src=\"" + nomer + ".jpg\"><br>";
								break;
							default:
								file = pathfolder + "/" + nomer + ".png";
								htmlCok += "<img src=\"" + nomer + ".png\"><br>";
								break;
						}
						try
						{
							using (var fs = File.Create(file))
							{
								streamImg.CopyTo(fs);
								sukses = true;
								Console.WriteLine("Completed");
							}
						}catch(Exception asu)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Write(file);
							Console.WriteLine(asu.ToString());
							Console.ForegroundColor = ConsoleColor.Green;
						}
					}
					else if (streamImg != null)
					{
						Console.WriteLine("Invalid");
					}
					else
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Gak konek internet cuk");
						Console.ForegroundColor = ConsoleColor.Green;
						Console.Read();
					}
					i++;
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}

		}
		static string removeIllegal(string asu)
		{
			string[] cok = new string[] { "<", ">", ":", "\"", "/", "\\", "|", "?", "*" };
			foreach (string q in cok)
			{
				asu = Regex.Replace(asu, Regex.Escape(q), string.Empty);
			}
			return asu;
		}
		static string htmlCok;
		static Stream getAudio(string curl)
		{
			if (curl.Contains("naverAudioPlayer"))
			{
				Match a = Regex.Match(curl, "trackInfos\\.push\\(\\{\\s*url :\\s*'(.*?)',", RegexOptions.Singleline);
				if (a.Groups.Count == 2)
				{
					Console.Write("[+] Mendownload m4a audio: ");
					string urljson = a.Groups[1].Value;
					string error1 = null;
					string getjson = getTextFromSteam(get(urljson, ref error1));
					if (error1 != null)
					{
						Console.ForegroundColor = ConsoleColor.Red;
						Console.WriteLine("Gagal mendapatkan file audio -> " + error1);
						Console.ForegroundColor = ConsoleColor.Green;
						return null;
					}
					else
					{
						JavaScriptSerializer js = new JavaScriptSerializer();
						Class1.RootObject mbuh = js.Deserialize<Class1.RootObject>(getjson);
						string urlaudio = mbuh.audioPlayUrlInfos[1].urls[3].url;
						error1 = null;

						Stream getaudio = get(urlaudio, ref error1);
						if (error1 != null)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine("Gagal mendapatkan file audio -> " + error1);
							Console.ForegroundColor = ConsoleColor.Green;
							return null;
						}
						else
						{
							Console.WriteLine("Sukses");
							return getaudio;
						}
					}
				}
				else
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("[-] Gagal mendapatkan file audio");
					Console.ForegroundColor = ConsoleColor.Green;
					return null;
				}
			}
			else return null;
		}
		static void proses(List<string> list, List<string> text, int begin, int end, bool timpa)
		{
			try
			{
				for (int i = begin; i <= end; i++)
				{
					string error = null;
					Console.WriteLine("[" + i + "] " + text[i]);
					string curl = null;
					int c = 0;
					do
					{
						curl= getTextFromSteam(get(list[i], ref error));
						c++;
					} while (string.IsNullOrEmpty(curl) && c <= 5);
					
					Match x = Regex.Match(curl, "id=\"_imageList\".*?\\s*(.*?)\\s*</div>", RegexOptions.IgnoreCase | RegexOptions.Singleline);
					if (x.Groups.Count < 2) continue;
					Regex r = new Regex("data-url=\"(.+?)\"", RegexOptions.IgnoreCase);
					MatchCollection matches = r.Matches(x.Groups[1].Value);
					int j = 0;
					string dir = removeIllegal(text[i]).Trim();
					if (!Directory.Exists(foldersimpan + "/" + dir)) Directory.CreateDirectory(foldersimpan + "/" + dir);
					htmlCok = "<html><head><title>" + dir + "</title></head><body bgcolor=\"#000000\"><center>";
					string pathfolder = foldersimpan + "/" + dir;
					foreach (Match m in matches)
					{

						string url = m.Groups[1].Value;
						Console.Write("[+] " + url + " -> ");
						if ((File.Exists(pathfolder + "/" + j + ".jpg") || File.Exists(pathfolder + "/" + j + ".png")) && !timpa)
						{
							string z = "jpg"; ;
							if (File.Exists(pathfolder + "/" + j + ".jpg")) z = "jpg";
							else if (File.Exists(pathfolder + "/" + j + ".png")) z = "png";
							htmlCok += "<img src=\"" + j + "." + z + "\"><br>";
							Console.WriteLine(" Sudah ada");
							j++;
							continue;
						}
						else
						{
							downloadGambar(url, j, pathfolder, list[i]);
							j++;
						}

					}
					if (File.Exists(pathfolder + "/" + dir + ".m4a") && !timpa)
					{
						htmlCok += "<audio controls autoplay hidden><source src=\"" + dir + ".m4a\" type=\"audio/mp4\" /><p>Your browser does not support HTML5 audio.</p></audio>";
						Console.WriteLine("[+] File " + dir + ".m4a sudah ada");
					}
					else
					{
						try
						{
							Stream audio = getAudio(curl);
							if (audio != null)
							{
								using (var fs = File.Create(pathfolder + "/" + dir + ".m4a"))
								{
									audio.CopyTo(fs);
								}
								htmlCok += "<audio controls autoplay hidden><source src=\""+dir+".m4a\" type=\"audio/mp4\" /><p>Your browser does not support HTML5 audio.</p></audio>";
							}
						}catch(Exception asu)
						{
							Console.ForegroundColor = ConsoleColor.Red;
							Console.WriteLine(asu.ToString());
							Console.ForegroundColor = ConsoleColor.Green;
						}

					}
					htmlCok += "</body></center></html>";
					try
					{
						File.WriteAllText(pathfolder + "/" + dir + ".html", htmlCok);
					}catch(Exception asu)
					{
						Console.WriteLine(asu.ToString());
					}

				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
			finally
			{
				Console.WriteLine("\n[+] Proses completed");
			}
		}
		static bool cekInputRange(List<string> list, string range, ref int begin, ref int end)
		{
			Regex r = new Regex("^(\\d+)\\-(\\d+)$");
			Match m = r.Match(range);
			int x = 0, y = 0;
			if (m.Success)
			{
				x = Convert.ToInt16(m.Groups[1].Value);
				y = Convert.ToInt16(m.Groups[2].Value);
				if (x > y)
				{
					Console.WriteLine("Angka pertama harus lebih kecil atau sama dengan angka terakhir");
					return false;
				}
				else if (x < 0 || y > list.Count - 1)
				{
					Console.WriteLine("Range harus diantara 0 sampai Range maksimal");
					return false;
				}
			}
			else
			{
				Console.WriteLine("Format salah njing");
				return false;
			}
			begin = x;
			end = y;
			return true;
		}

	}
}
