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
		this.url = url;
	}

	public static void main(String[] args) 
	{
		Program program = new Program(Integer.parseInt(args[0]), Integer.parseInt(args[1]), args[2], args[3]);
		program.Run();
	}

	public void Run()
	{
		CreateFolderIfNecessary("images");
		CreateFolderIfNecessary(name);
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

    public void CreateFolderIfNecessary(String folder_name)
    {
        File folder = new File(folder_name);
        boolean folder_condition = folder.exists() && folder.isDirectory();

        if(folder_condition == false)
        {
        	folder.mkdir();
        }
    }
}