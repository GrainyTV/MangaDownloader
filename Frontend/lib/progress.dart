import 'dart:io';
import 'dart:async';
import 'dart:convert';
import 'package:flutter/material.dart';

class Fraction
{
	int num;
	int den;

	Fraction({ required this.num, this.den = 1 });

	void Add(Fraction operand)
	{
		num += operand.num;
	}

	double Value()
	{
		return num / den;
	}

	void Print()
	{
		print('[$num|$den]');
	}
}

class Progress extends StatefulWidget
{
	final Function() next;
	final String firstChapter;
	final String lastChapter;
	final String title;
	final String link;	
	late Fraction increment; 
	late Fraction percantage;

	int countOccurences(String mainString, String search)
	{
 		int lInx = 0;
 		int count = 0;
 		
 		while(lInx != -1)
 		{
 			lInx = mainString.indexOf(search, lInx);
 			
 			if(lInx != -1)
 			{
 				++count;
 				lInx += search.length;
 			}
 		}
		
		return count;
	}

	Progress({ required this.next, required this.firstChapter, this.lastChapter = 'undefined', required this.title, required this.link });

	@override
	_ProgressState createState() => _ProgressState();
}

class _ProgressState extends State<Progress> 
{
	void renderPDFs() async
	{
		var process = await Process.start('java',
		[
			'-cp',
			'.:lib/*',
			'Program',
			'${widget.firstChapter}',
			'${widget.lastChapter}',
			'${widget.title}',
			'${widget.link}'
		], workingDirectory: '${Directory.current.path}/lib/java');

		process.stdout.transform(utf8.decoder).forEach( (s) =>
		{ 
			setState( () { for(int i = 0; i < widget.countOccurences(s, '[-]'); ++i) { widget.percantage.Add(widget.increment); widget.percantage.Print(); } }) 
		});
  	}

	void renderPDF() async
	{
		var process = await Process.start('java',
		[
			'-cp',
			'.:lib/*',
			'Program',
			'${widget.firstChapter}',
			'${widget.title}',
			'${widget.link}'
		], workingDirectory: '${Directory.current.path}/lib/java');

		process.stdout.transform(utf8.decoder).forEach(print);
  	}

  	@override
  	void initState()
  	{
  		super.initState();

  		if(widget.lastChapter == 'undefined')
  		{
  			renderPDF();
  		}
  		else
  		{
  			int denumerator = (int.parse(widget.lastChapter) - int.parse(widget.firstChapter)) + 1;
  			widget.increment = new Fraction(num: 1, den: denumerator);
  			widget.percantage = new Fraction(num: 0, den: denumerator);
  			renderPDFs();
  		}	
  	}

  	@override
	Widget build(BuildContext context) 
	{
  		return Column
		(
			children: <Widget>
            [
                Expanded
                (
                	flex: 1,
                	child: Container
	                (
	        			/*alignment: Alignment.centerRight,
	            		margin: EdgeInsets.fromLTRB(3, 3, 6, 3),
	            		child: Container
	            		(
	            			decoration: BoxDecoration
		            		(
		                		border: Border.all(color: Colors.black, width: 2.0),
		                		borderRadius: BorderRadius.circular(50),
		                	),
		                	
		            		child: InkWell
		            		(
		                		onTap: widget.close,
		                		child: Icon(Icons.close)
		                	)
	            		)*/	
	                )
                ),

                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		alignment: Alignment.center,
                		child: Text('In Progress', style: TextStyle(fontSize: 30, fontFamily: 'Tisa', fontWeight: FontWeight.bold))
                	)
                ),
                
                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		alignment: Alignment.center,
                		padding: EdgeInsets.all(20),
                		child: LinearProgressIndicator
                		(
                			value: widget.percantage.Value()
                		)
                	)
                ),

                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		alignment: Alignment.center,
                		child: Text('Your files are created in the background.', style: TextStyle(fontSize: 20), textAlign: TextAlign.center)
                	)
                ),

                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                	)
                ),

                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		padding: EdgeInsets.symmetric(vertical: 25),
                		child: ElevatedButton
                		(
            				style: ElevatedButton.styleFrom(textStyle: TextStyle(fontSize: 20), backgroundColor: Color(0xff152238)),
            				onPressed: (widget.percantage.Value() >= 1) ? widget.next : null,
            				child: Text('Finish'),
          				),
                	)
                ),
            ]
		);
	}
}