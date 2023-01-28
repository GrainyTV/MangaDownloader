import java.util.concurrent.ExecutionException;
import java.util.concurrent.FutureTask;
import org.jsoup.HttpStatusException;
import org.jsoup.select.Elements;
import org.jsoup.nodes.Document;
import org.jsoup.nodes.Element;
import java.io.IOException;
import java.util.ArrayList;
import java.util.HashMap;
import org.jsoup.Jsoup;
import java.util.List;
import java.util.Map;


class Preparation implements Runnable
{
	private final String name;
	private final String url;
	private final int chapter;
	private final Map<String, List<String>> map;
	private int[][] dimensions;
	private String extension;

	public Preparation(String name, String url, int chapter)
	{
		this.name = name;
		this.url = url;
		this.chapter = chapter;
		this.map = new HashMap<>();
	}

	public void run()
	{
		try
		{		
			ExtractNecessaryPageData();
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}

	public void ExtractNecessaryPageData()
	{
		boolean success;

		do
		{
			success = true;

			try
			{
				/*Document document = (ContainsPlaceholder())
				? Jsoup.connect(InsertChapterIntoURL()).userAgent("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36").timeout(60000).get()
				: Jsoup.connect(url).userAgent("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36").timeout(60000).get();
				Elements images = document.getElementsByTag("img");
				
				DisposeUnnecessaryImages(images);
				StartImageProcess(images);
				CreatePDF();*/

				Document document = Jsoup.connect("https://edu.vik.bme.hu/mod/quiz/review.php?attempt=394772&cmid=74264").userAgent("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36").timeout(60000).get();
				System.out.println(document);
			}
			catch(HttpStatusException e)
			{
				if(e.getStatusCode() == 503)
				{
					success = false;
				}		
			}
			catch(IOException e)
			{
				e.printStackTrace();
			}

		} while(success == false);		
	}

	public void DisposeUnnecessaryImages(Elements images)
	{
		ThreadGroup threadGroup = new ThreadGroup("Image Parser Threads");
		List<FutureTask<String[]>> ft_list = new ArrayList<>();

		for(Element img : images)
		{
			ft_list.add(new FutureTask<String[]>(new Dispose(img)));
			Thread t = new Thread(threadGroup, ft_list.get(ft_list.size() - 1));
			t.start();
		}

		do
		{
			for(int i = 0; i < ft_list.size(); ++i)
			{
				if(ft_list.get(i).isDone())
				{
					try
					{
						FillMapWithValues(ft_list.get(i).get());
					}
					catch(InterruptedException | ExecutionException e)
					{			
					}

					ft_list.remove(i);
				}
			}

		} while(ft_list.size() > 0);	
	}

	public void StartImageProcess(Elements images)
	{
		ThreadGroup threadGroup = new ThreadGroup("Image Downloader Threads");
		String key = RequiredDiv();
		int length = map.get(key).size();
		
		Sort(key, length, images);
		
		extension = ImageExtension(map.get(key).get(0));
		dimensions = new int[length][2];

		for(int i = 0; i < length; ++i)
		{
			Thread t = new Thread(threadGroup, new Image(map.get(key).get(i), i, ImageLocationGenerator(i), dimensions));
			t.start();
		}

		while(threadGroup.activeCount() > 0)
		{	
		}
	}

	public void CreatePDF()
	{
		Thread t = new Thread(new PDF(PDFLocationGenerator(), ImageLocationGenerator(0), dimensions));
		t.start();
	}

    public String RequiredDiv()
    {
    	int items = 0;
    	String divName = new String();

		for(var keyValuePair : map.entrySet())
		{
			if(keyValuePair.getValue().size() > items)
			{
				items = keyValuePair.getValue().size();
				divName = keyValuePair.getKey();
			}
		}

		return divName;
    }

    public String ImageExtension(String url)
    {
    	int firstIndex = url.lastIndexOf(".") + 1;
    	int lastIndex = url.length();
    	return url.substring(firstIndex, lastIndex);
    }

    public String InsertChapterIntoURL()
    {
    	StringBuilder sb = new StringBuilder();
    	int idx1 = url.indexOf("{");
    	int idx2 = url.indexOf("}") + 1;

    	sb.append(url);
    	sb.replace(idx1, idx2, Integer.toString(chapter));

    	return sb.toString();
    }

    public void FillMapWithValues(String[] arr)
    {
		if(map.containsKey(arr[0]))
		{
			map.get(arr[0]).add(arr[1]);
		}
		else
		{
			map.put(arr[0], new ArrayList<String>());
			map.get(arr[0]).add(arr[1]);
		}
    }

	public String ImageLocationGenerator(int name)
	{
		StringBuilder output = new StringBuilder();
		
		output.append("images/");
		output.append(chapter);
		output.append('/');
		output.append(name);
		output.append('.');
		output.append(extension);

		return output.toString();
	}

	public String PDFLocationGenerator()
	{
		StringBuilder output = new StringBuilder();
		
		output.append(name);
		output.append('/');
		output.append(name);
		output.append(" Chapter ");
		output.append(chapter);

		return output.toString();
	}

	public void Sort(String key, int length, Elements images)
	{
		List<String> valid_order = new ArrayList<>();
		List<String> values = map.get(key);

		for(int i = 0; i < images.size(); ++i)
		{
			if(values.contains(images.get(i).attr("data-src")))
			{
				valid_order.add(images.get(i).attr("data-src"));
			}
			else if(values.contains(images.get(i).attr("src")))
			{
				valid_order.add(images.get(i).attr("src"));
			}
			else
			{
				continue;
			}
		}

		map.put(key, valid_order);
	}

	public boolean ContainsPlaceholder()
	{
		if(url.contains("{0}"))
		{
			return true;
		}

		return false; 
	}
}