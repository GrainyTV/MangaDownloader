import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.pdfbox.pdmodel.PDPageContentStream;
import org.apache.pdfbox.pdmodel.common.PDRectangle;
import java.util.concurrent.ExecutorService;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.PDPage;
import java.net.MalformedURLException;
import java.awt.image.BufferedImage;
import java.net.HttpURLConnection;
import org.jsoup.select.Elements;
import org.jsoup.nodes.Element;
import java.text.MessageFormat;
import javax.imageio.ImageIO;
import java.io.IOException;
import java.io.InputStream;
import org.jsoup.Jsoup;
import java.io.File;
import java.net.URL;
  
class Program implements Runnable
{
	private String input;
	private String name;
	private String extension;
	private int[][] image_size;

	public Program(String input, String name, String extension, int[][] image_size)
	{
		this.input = input;
		this.name = name;
		this.extension = extension;
		this.image_size = image_size;
	}

	public void run()
	{
		try
		{		
			SaveIMG(input, name, extension, image_size);
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}

	public static void main(String[] args) 
	{
		final int FIRST_CHAPTER = 1;
		final int FINAL_CHAPTER = 10;
		final String NAME = "Shadows House";
		final String LINK = "https://ww4.mangakakalot.tv/chapter/manga-di980617/chapter-{0}";
		final String IMAGE_CONTAINER_DIV = "div[class=vung-doc]";
		final String IMAGE_FILE_EXTENSION = ".jpg";

		try
		{
			Run(FIRST_CHAPTER, FINAL_CHAPTER, NAME, LINK, IMAGE_CONTAINER_DIV, IMAGE_FILE_EXTENSION);
		}
		catch(IOException e)
		{
			e.printStackTrace();
		}
	}

	public static void MergePDF(String title, String fullname, int[][] image_size)
	{
		PDDocument pdfdoc = new PDDocument();

		File dir = new File(System.getProperty("user.dir"), "images");
  		File[] directoryListing = dir.listFiles();
  		
  		for(int i = 0; i < directoryListing.length; ++i)
  		{
  			for(int j = i + 1; j < directoryListing.length; ++j)
  			{
  				int ffd = directoryListing[i].getName().indexOf(".");
  				int sfd = directoryListing[j].getName().indexOf(".");

  				if(Integer.parseInt(directoryListing[i].getName().substring(0, ffd)) > Integer.parseInt(directoryListing[j].getName().substring(0, sfd)))
  				{
  					File tmp = directoryListing[i];
  					directoryListing[i] = directoryListing[j];
  					directoryListing[j] = tmp;
  				}
  			}
  		}

  		int i = 0;

		for(File child : directoryListing)
		{
			pdfdoc.addPage(new PDPage(new PDRectangle(image_size[i][0], image_size[i][1])));
			PDPage page = pdfdoc.getPage(i);
			
			try
			{
				PDImageXObject pdImage = PDImageXObject.createFromFile(child.getPath(), pdfdoc);
				PDPageContentStream contentStream = new PDPageContentStream(pdfdoc, page);
				contentStream.drawImage(pdImage, 0, 0);
				contentStream.close();
			}
			catch(IOException e)
			{
				e.printStackTrace();
			}
			
			++i;
	    }
	
		try
		{
			File f = new File(MessageFormat.format("{0}", title));

			if(f.exists() && f.isDirectory())
			{
				pdfdoc.save(MessageFormat.format("{0}/{1}.pdf", title, fullname));
			}
			else
			{
				f.mkdir();
				pdfdoc.save(MessageFormat.format("{0}/{1}.pdf", title, fullname));
			}
			   
			pdfdoc.close();
		}
		catch(IOException e)
		{
			e.printStackTrace();
		}

		for(File child : directoryListing)
		{
			child.delete();
	    }
	}

	public static void SaveIMG(String input, String name, String extension, int[][] image_size) throws IOException, MalformedURLException
	{
		ImageIO.setUseCache(false);
		URL url = new URL(input);
        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
        conn.setRequestMethod("GET");
        conn.setRequestProperty("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
        InputStream is = conn.getInputStream();
        BufferedImage image = ImageIO.read(is);
        BufferedImage alphaless_image;

        final int index = Integer.parseInt(name);
    	image_size[index][0] = image.getWidth();
    	image_size[index][1] = image.getHeight();

        if(image.getColorModel().hasAlpha())
        {
        	alphaless_image = new BufferedImage(image_size[index][0], image_size[index][1], BufferedImage.TYPE_INT_RGB);
        	alphaless_image.getGraphics().drawImage(image, 0, 0, null);
        }
        else
        {
        	alphaless_image = image;
        }

        File f = new File("images");

        if(f.exists() && f.isDirectory())
        {
        	ImageIO.write(alphaless_image, extension.substring(1, extension.length()), new File(MessageFormat.format("images/{0}{1}", name, extension)));
        }
        else
        {
        	f.mkdir();
        	ImageIO.write(alphaless_image, extension.substring(1, extension.length()), new File(MessageFormat.format("images/{0}{1}", name, extension)));
        }
        
        is.close();
	}

	public static void Run(int first_chapter, int final_chapter, String name, String url, String image_container, String image_extension) throws IOException
	{
		for(int j = first_chapter - 1; j < final_chapter; ++j)
		{
			long startTime = System.nanoTime();
			var document = Jsoup.connect(MessageFormat.format(url, j + 1)).timeout(60000).userAgent("Chrome").get();
			Element section = document.select(image_container).first();
			Elements images = section.select(MessageFormat.format("img[data-src$={0}]", image_extension));
			var dimensions = new int[images.size()][2];

			for(int i = 0; i < images.size(); ++i)
			{
				Thread t = new Thread(new Program(images.get(i).attr("data-src"), Integer.toString(i), image_extension, dimensions));
				t.start();
			}

			while(Thread.activeCount() > 1)
			{	
			}

			String fullname = MessageFormat.format("{0} Chapter {1}", name, j + 1);
			Thread t = new Thread(new Runnable()
			{
				public void run()
				{
					MergePDF(name, fullname, dimensions);
				}
		 	});
			t.start();

			long endTime = System.nanoTime();
			long duration = (endTime - startTime) / 1000000;
			System.out.println(MessageFormat.format("{0}.pdf is finished.[{1} MS]", fullname, duration));
		}
	}
}