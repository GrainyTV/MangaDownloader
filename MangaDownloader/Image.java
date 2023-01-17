import org.apache.commons.imaging.ImageReadException;
import org.apache.commons.imaging.ImageInfo;
import org.apache.commons.imaging.Imaging;
import java.nio.file.StandardCopyOption;
import java.io.IOException;
import java.io.InputStream;
import java.nio.file.Files;
import java.nio.file.Paths;
import java.net.URL;

class Image implements Runnable
{
	private final String image_url;
	private final int page;
	private final String location;
	private final int[][] image_sizes;

	public Image(String image_url, int page, String location, int[][] image_sizes)
	{
		this.image_url = image_url;
		this.page = page;
		this.location = location;
		this.image_sizes = image_sizes;
	}

	public void run()
	{
		try
		{		
			SaveIMG();
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}

	public void SaveIMG() throws IOException
	{
 		InputStream is = new URL(image_url).openStream();  
    	Files.copy(is, Paths.get(location), StandardCopyOption.REPLACE_EXISTING);

        try
        {
        	ImageInfo imageInfo = Imaging.getImageInfo(Files.readAllBytes(Paths.get(location)));
    		image_sizes[page][0] = imageInfo.getWidth();
    		image_sizes[page][1] = imageInfo.getHeight();
        }
        catch(ImageReadException e)
        {
        	e.printStackTrace();
        }

        is.close();  
	}
}