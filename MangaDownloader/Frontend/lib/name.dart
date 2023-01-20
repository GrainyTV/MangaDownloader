import 'package:flutter/material.dart';

class Name extends StatefulWidget 
{
	final Function() close;
	final Function() next;
	String title = '';
	bool buttonEnabled = false;

	Name({ required this.close, required this.next });

	String GetName()
	{
		return title;
	}

	@override
	Name_State createState() => Name_State();
}

class Name_State extends State<Name> 
{
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
	        			alignment: Alignment.centerRight,
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
	            		)		
	                )
                ),

                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		alignment: Alignment.center,
                		child: Text('Title', style: TextStyle(fontSize: 30, fontFamily: 'Tisa', fontWeight: FontWeight.bold))
                	)
                ),
                
                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		alignment: Alignment.center,
                		padding: EdgeInsets.all(20),
                		child: TextField
                		(
                			onChanged: (input) { setState( () { widget.title = input; widget.buttonEnabled = (!widget.title.isEmpty) ? true : false; }); },
                			maxLines: 1,
                			style: TextStyle(color: Color(0xff152238)),
                			cursorColor: Color(0xff152238),
                			decoration: InputDecoration
                			(
                				contentPadding: EdgeInsets.fromLTRB(10, 0, 10, -12),
                				enabledBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
					        	focusedBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
					        	label: Text('Given Name:', style: TextStyle(color: Color(0xff152238)))
					    	),
                		)
                	)
                ),

                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		alignment: Alignment.center,
                		child: Text('Enter a title for your manga.', style: TextStyle(fontSize: 20), textAlign: TextAlign.center)
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
            				onPressed: (widget.buttonEnabled) ? widget.next : null,
            				child: Text('Next'),
          				),
                	)
                ),
            ]
		);
	}
}
