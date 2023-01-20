import org.apache.pdfbox.pdmodel.graphics.image.PDImageXObject;
import org.apache.pdfbox.pdmodel.PDPageContentStream;
import org.apache.pdfbox.pdmodel.common.PDRectangle;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.PDPage;
import java.io.IOException;
import java.io.File;

class PDF implements Runnable
{
	private final String fullname;
	private final String location;
	private final int[][] image_size;

	public PDF(String fullname, String location, int[][] image_size)
	{
		this.fullname = fullname;
		this.location = location;
		this.image_size = image_size;
	}

	public void run()
	{
		try
		{
			MergePDF();
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}

	public void MergePDF()
	{
		PDDocument pdfdoc = new PDDocument();
		File dir = new File(System.getProperty("user.dir"), LocationGenerator());
  		File[] directoryListing = dir.listFiles();
		
		BubbleSort(directoryListing);
		FillPDFWithContent(pdfdoc, directoryListing);
		SaveAndCleanResources(pdfdoc, dir, directoryListing);
	}

	public void BubbleSort(File[] arr)
	{
  		for(int i = 0; i < arr.length; ++i)
  		{
  			for(int j = i + 1; j < arr.length; ++j)
  			{
  				int ffd = arr[i].getName().indexOf(".");
  				int sfd = arr[j].getName().indexOf(".");

  				if(Integer.parseInt(arr[i].getName().substring(0, ffd)) > Integer.parseInt(arr[j].getName().substring(0, sfd)))
  				{
  					File tmp = arr[i];
  					arr[i] = arr[j];
  					arr[j] = tmp;
  				}
  			}
  		}
	}

	public void FillPDFWithContent(PDDocument doc, File[] arr)
	{
  		int i = 0;

		for(File child : arr)
		{
			doc.addPage(new PDPage(new PDRectangle(image_size[i][0], image_size[i][1])));
			PDPage page = doc.getPage(i);
			
			try
			{
				PDImageXObject pdImage = PDImageXObject.createFromFile(child.getPath(), doc);
				PDPageContentStream contentStream = new PDPageContentStream(doc, page);
				contentStream.drawImage(pdImage, 0, 0);
				contentStream.close();
			}
			catch(IOException e)
			{
				e.printStackTrace();
			}
			
			++i;
	    }
	}

	public void SaveAndCleanResources(PDDocument doc, File image_directory, File[] arr)
	{
	    try
	    {
	    	doc.save(fullname);
	    	doc.close();
	    	System.out.println("[-]");
	    }
	    catch(IOException e)
	    {
	    	e.printStackTrace();
	    }

	    for(File child : arr)
	    {
	    	child.delete();
	    }

		image_directory.delete();
	}

	public String LocationGenerator()
	{
		int idx = location.lastIndexOf("/");
		return location.substring(0, idx);
	}
}