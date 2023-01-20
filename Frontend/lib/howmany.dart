import 'package:flutter/material.dart';

class HowMany extends StatefulWidget
{
	final Function() close;
	final Function() next;
	int amount;
	bool buttonEnabled1 = false;
	bool buttonEnabled2 = false;
	String input1 = '';
	String input2 = '';

	HowMany({ required this.close, required this.next, this.amount = -1 });

	String GetChapterValue({ bool first = true })
	{
		if(first == true)
		{
			return input1;
		}

		return input2;
	}

	@override
	HowMany_State createState() => HowMany_State();
}

class HowMany_State extends State<HowMany>
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
                		child: Text('Amount', style: TextStyle(fontSize: 30, fontFamily: 'Tisa', fontWeight: FontWeight.bold))
                	)
                ),
                
                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		alignment: Alignment.center,
                		padding: EdgeInsets.symmetric(horizontal: 20, vertical: 20),
                		child: Column
                		(
      						children: <Widget>
      						[
      							Expanded
      							(
      								flex: 2,
	      							child: Row
	      							(
	      								children: <Widget>
	      								[
	      									Expanded
	      									(
	      										flex: 10,
						                		child: TextField
						                		(
						                			onChanged: (input) { setState( () 
						                			{ 
						                				var value = int.tryParse(input);

						                				if(value != null && value > 0)
						                				{
						                					widget.input1 = input;
						                					widget.buttonEnabled1 = true;
						                				}
						                				else
						                				{
						                					widget.buttonEnabled1 = false;
						                				}

						                			}); },
						                			maxLines: 1,
						                			style: TextStyle(color: Color(0xff152238)),
						                			cursorColor: Color(0xff152238),
						                			decoration: InputDecoration
						                			(
						                				contentPadding: EdgeInsets.fromLTRB(10, 0, 10, -12),
						                				enabledBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
											        	focusedBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
											        	label: Text('First:', style: TextStyle(color: Color(0xff152238)))
											    	),
						                		)
	      									),

	      									Expanded(flex: 1, child: Container()),

	      									Expanded
	      									(
	      										flex: 10,
						                		child: TextField
						                		(
						                			onChanged: (input) { setState( () 
						                			{ 
						                				var value = int.tryParse(input);

						                				if(value != null && value > 0)
						                				{
						                					var previous = int.tryParse(widget.input1);

						                					if(previous != null && previous > 0 && value > previous)
						                					{
						                						widget.input2 = input;
						                						widget.buttonEnabled2 = true;
						                					}
						                					else
						                					{
						                						widget.buttonEnabled2 = false;
						                					}
						                				}
						                				else
						                				{
						                					widget.buttonEnabled2 = false;
						                				} 

						                			}); },
						                			maxLines: 1,
						                			style: TextStyle(color: Color(0xff152238)),
						                			cursorColor: Color(0xff152238),
						                			decoration: InputDecoration
						                			(
						                				contentPadding: EdgeInsets.fromLTRB(10, 0, 10, -12),
						                				enabledBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
											        	focusedBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
											        	label: Text('Last:', style: TextStyle(color: Color(0xff152238)))
											    	),
						                		)
	      									)
	      								]
	      							),
      							),
        					]
    					)
                	)
                ),

                Expanded
                (
                	flex: 2,
                	child: Container
                	(
                		alignment: Alignment.center,
                		child: Text('Enter the number of the first and last chapter you will read.', style: TextStyle(fontSize: 20), textAlign: TextAlign.center)
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
            				onPressed: (widget.buttonEnabled1 && widget.buttonEnabled2) ? widget.next : null,
            				child: Text('Next'),
          				),
                	)
                ),
            ]
		);
	}
}