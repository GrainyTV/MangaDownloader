import 'package:flutter/material.dart';
import 'name.dart';
import 'amount.dart';
import 'howmany.dart';
import 'link.dart';
import 'progress.dart';
import 'which.dart';

enum Phase { main, name, amount, howmany, link, which, progress }

class ContainerContent extends StatelessWidget
{
	final Widget current;
	final bool hidden;

	ContainerContent({ required this.current, this.hidden = false });

	@override
	Widget build(BuildContext context)
	{
		return Visibility
		(
			visible: !hidden,
			child: Container
        	( 
	            decoration: BoxDecoration
	            (
	                boxShadow: [ BoxShadow(blurRadius: 25, blurStyle: BlurStyle.outer) ],
	                color: Color(0xefffffff)
	            ),
	            child: current
        	)
        );
	}
}


class Panel extends StatefulWidget 
{
	final Function() close;
	Phase phase;

	var name;
	var amount;
	var howmany;
	var link;
	var which;

    String FindOutChapterNumber(String url)
    {
    	List<String> url_parts = url.toLowerCase().split('/'); 

    	for(int i = url_parts.length - 1; i > 2; --i)
    	{
    		String current = url_parts[i];

    		if(current.contains('chapter'))
    		{
    			int start_index = current.indexOf('chapter');
    			int final_index = current.length;
    			String part_after_chapter = current.substring(start_index + 7, final_index);

    			if(ContainsChapterNumber(part_after_chapter))
    			{
    				return ExtractChapterNumber(part_after_chapter);
    			}
    		}
    	}

    	throw new FormatException("Provided Link Does Not Contain Chapter Number!");
    }

    bool ContainsChapterNumber(String input)
	{
      	List<String> chars = input.split('');
      
      	for(String c in chars)
      	{
         	if(int.tryParse(c) != null)
         	{
            	return true;
         	}
      	}

      	return false;
   	}

   	String ExtractChapterNumber(String input)
   	{
   		StringBuffer sb = new StringBuffer();
      	List<String> chars = input.split('');
      	bool first_occurence = false;
      
      	for(String c in chars)
      	{
         	if(first_occurence == false && int.tryParse(c) == null)
         	{
            	continue;
         	}
         	else if(first_occurence == false && int.tryParse(c) != null)
         	{
         		sb.write(c);
         		first_occurence = true;
         	}
         	else if(first_occurence == true && int.tryParse(c) != null)
         	{
         		sb.write(c);
         	}
         	else
         	{
         		break;
         	}
      	}

      	return sb.toString();
   	}

   	void Reset()
   	{
   		name = null;
   		amount = null;
   		howmany = null;
   		link = null;
   		which = null;
   	}

	Panel({ required this.phase, required this.close });

	@override
	Panel_State createState() => Panel_State();
}

class Panel_State extends State<Panel>
{
	@override
	Widget build(BuildContext context)
	{
		switch (widget.phase)
		{
			case Phase.name:
				widget.name = Name(close: widget.close, next: () { setState( () { widget.phase = Phase.amount; }); });
				return ContainerContent(current: widget.name);
			
			case Phase.amount:
				widget.amount = Amount(close: widget.close, next: () { setState( () { widget.phase = Phase.howmany; }); });
				return ContainerContent(current: widget.amount);
			
			case Phase.howmany:
				if(widget.amount.GetChoice() == Choice.one)
				{
					widget.phase = Phase.link;
					widget.link = Link(close: widget.close, next: ()
					{
						try
						{
							widget.howmany = widget.FindOutChapterNumber(widget.link.GetLink());
							setState( () { widget.phase = Phase.progress; });
						}
						on FormatException catch(e)
						{
							setState( () { widget.phase = Phase.which; });
						} 
					});
					return ContainerContent(current: widget.link);
				}

				widget.howmany = HowMany(close: widget.close, next: () { setState( () { widget.phase = Phase.link; }); });
				return ContainerContent(current: widget.howmany);

			case Phase.link:
				widget.link = Link(close: widget.close, next: () { setState( () { widget.phase = Phase.progress; }); });
				return ContainerContent(current: widget.link);

			case Phase.which:
				widget.which = Which(close: widget.close, next: () { setState( () { widget.phase = Phase.progress; }); });
				return ContainerContent(current: widget.which);

			case Phase.progress:
				return (widget.amount.GetChoice() == Choice.one) ? ContainerContent(current: Progress
				(
					next: () { setState( () {  widget.Reset(); widget.phase = Phase.main; }); },
					firstChapter: (widget.howmany != null) ? widget.howmany : widget.which.GetChapterValue(),
					title: widget.name.GetName(),
					link: widget.link.GetLink()					
				)) : ContainerContent(current: Progress
				(
					next: () { setState( () { widget.Reset(); widget.phase = Phase.main; }); },
					firstChapter: widget.howmany.GetChapterValue(),
					lastChapter: widget.howmany.GetChapterValue(first: false),
					title: widget.name.GetName(),
					link: widget.link.GetLink()
				));

			case Phase.main:
			default:
				return ContainerContent(current: Container(), hidden: true);
		}
	}
}

