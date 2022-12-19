import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.pdfbox.pdmodel.PDPageContentStream;
import org.apache.pdfbox.pdmodel.common.PDRectangle;
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
  
class Program
{
	public static void main(String[] args) 
	{
		final int FIRST_CHAPTER = 1;
		final int FINAL_CHAPTER = 181;
		final String NAME = "The Promised Neverland";
		final String LINK = "https://www44.promised-neverland.com/manga/the-promised-neverland-chapter-{0}";
		final String IMAGE_CONTAINER_DIV = "div[class=entry-content]";
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

	public static void MergePDF(String title, String fullname)
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
			try
			{
				BufferedImage img = ImageIO.read(child);
				pdfdoc.addPage(new PDPage(new PDRectangle(img.getWidth(), img.getHeight())));
			}
			catch(IOException e)
			{
				e.printStackTrace();
			}
			
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
			System.out.println(MessageFormat.format("{0}.pdf is finished.", fullname));
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

	public static void SaveIMG(String input, String name, String extension) throws IOException, MalformedURLException
	{
		URL url = new URL(input);
		HttpURLConnection conn = (HttpURLConnection) url.openConnection();
		conn.setRequestMethod("GET");
		conn.setRequestProperty("User-Agent", "Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36");
		InputStream is = conn.getInputStream();
		BufferedImage image = ImageIO.read(is);
		BufferedImage alphaless_image = new BufferedImage(image.getWidth(), image.getHeight(), BufferedImage.TYPE_INT_RGB);
		alphaless_image.getGraphics().drawImage(image, 0, 0, null);
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
			var document = Jsoup.connect(MessageFormat.format(url, j + 1)).timeout(60000).userAgent("Chrome").get();
			Element section = document.select(image_container).first();
			Elements images = section.select(MessageFormat.format("img[data-src$={0}]", image_extension));

			for(int i = 0; i < images.size(); ++i)
			{
				try
				{
					SaveIMG(images.get(i).attr("data-src"), Integer.toString(i), image_extension);
				}
				catch(Exception e)
				{
					e.printStackTrace();
				}
			}

			MergePDF(name, MessageFormat.format("{0} Chapter {1}", name, j + 1));
		}
	}
}
