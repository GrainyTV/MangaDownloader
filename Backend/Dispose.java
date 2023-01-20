import java.util.concurrent.FutureTask;
import java.net.MalformedURLException;
import java.util.concurrent.Callable;
import org.jsoup.nodes.Element;
import java.net.URL;

class Dispose implements Callable<String[]>
{
	private final Element image;

	public Dispose(Element image)
	{
		this.image = image;
	}

	public String[] call()
	{	
		return LinkValidator();
	}

	public String[] LinkValidator()
	{
		String link = image.attr(DataSrcOrSrc(image));

		if(IsMatch(link))
		{
			Element parent = image.parent();

			while(parent.tagName() != "div")
			{
				parent = parent.parent();
			}

			return new String[] { parent.attr("class"), link };
		}

		throw new IllegalArgumentException();
	}

	public boolean IsMatch(String text)
	{
        try
        {
            URL url = new URL(text);
            return true;
        }
        catch(MalformedURLException e)
        {
        	return false;
    	}
    }

    public String DataSrcOrSrc(Element e)
    {
    	if(!e.attr("data-src").isEmpty())
    	{
    		return "data-src";
    	}

    	return "src";
    }
}