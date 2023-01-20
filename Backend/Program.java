import java.lang.ThreadGroup;
import java.io.File;

class Program
{
	private final int first_chapter;
	private final int final_chapter;
	private final String name;
	private final String url;

	public Program(int first_chapter, int final_chapter, String name, String url)
	{
		this.first_chapter = first_chapter;
		this.final_chapter = final_chapter;
		this.name = name;
		this.url = InsertPlaceholderIntoURL(url);
	}

	public Program(int chapter, String name, String url)
	{
		this.first_chapter = chapter;
		this.final_chapter = -1;
		this.name = name;
		this.url = url;
	}

	public static void main(String[] args) 
	{
		Program program;

		if(args.length == 4)
		{
			program = new Program(Integer.parseInt(args[0]), Integer.parseInt(args[1]), args[2], args[3]);
		}
		else
		{
			program = new Program(Integer.parseInt(args[0]), args[1], args[2]);	
		}

		program.Run();
	}

	public void Run()
	{
		CreateFolderIfNecessary("images");
		CreateFolderIfNecessary(name);

		if(final_chapter != -1)
		{
			ThreadGroup threadGroup = new ThreadGroup("Chapter Threads");

			for(int i = first_chapter; i <= final_chapter; ++i)
			{
				CreateFolderIfNecessary("images/" + i);
				
				Thread t = new Thread(threadGroup, new Preparation(name, url, i));
				t.start();

				while(threadGroup.activeCount() == 100)
				{	
				}
			}
		}
		else
		{
			CreateFolderIfNecessary("images/" + first_chapter);

			Thread t = new Thread(new Preparation(name, url, first_chapter));
			t.start();
		}
	}

    public void CreateFolderIfNecessary(String folder_name)
    {
        File folder = new File(folder_name);
        boolean folder_condition = folder.exists() && folder.isDirectory();

        if(folder_condition == false)
        {
        	folder.mkdir();
        }
    }

    public String InsertPlaceholderIntoURL(String url)
    {
    	StringBuilder sb = new StringBuilder();
    	String[] url_parts = url.toLowerCase().split("/"); 

    	for(int i = url_parts.length - 1; i > 2; --i)
    	{
    		String current = url_parts[i];

    		if(current.contains("chapter"))
    		{
    			int start_index = current.indexOf("chapter");
    			int final_index = current.length();
    			String part_before_chapter = current.substring(0, start_index + 7);
    			String part_after_chapter = current.substring(start_index + 7, final_index);

    			if(ContainsChapterNumber(part_after_chapter))
    			{
    				String changed = ReplaceChapterNumber(part_after_chapter);
    				
    				for(String s : url_parts)
    				{
    					if(s.equals(current))
    					{
    						sb.append(part_before_chapter);
    						sb.append(changed);
    					}
    					else
    					{
    						sb.append(s);
    					}

    					sb.append('/');
    				}

    				sb.deleteCharAt(sb.length() - 1);

    				break;
    			}
    		}
    	}

    	return sb.toString();
    }

	public boolean ContainsChapterNumber(String input)
	{
      	char[] chars = input.toCharArray();
      
      	for(char c : chars)
      	{
         	if(Character.isDigit(c))
         	{
            	return true;
         	}
      	}

      	return false;
   	}

   	public String ReplaceChapterNumber(String input)
   	{
   		StringBuilder sb = new StringBuilder();
      	char[] chars = input.toCharArray();
      	boolean first_occurence = false;
      
      	for(char c : chars)
      	{
         	if(Character.isDigit(c) == false)
         	{
            	sb.append(c);
         	}
         	else if(first_occurence == false && Character.isDigit(c))
         	{
         		sb.append('{');
         		sb.append(0);
         		sb.append('}');
         		first_occurence = true;
         	}
         	else
         	{
         		continue;
         	}
      	}

      	return sb.toString();
   	}
}