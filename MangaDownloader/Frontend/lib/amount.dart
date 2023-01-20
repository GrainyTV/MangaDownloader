import 'package:flutter/material.dart';

enum Choice { one, many }

class Amount extends StatefulWidget 
{
	final Function() close;
	final Function() next;
	Choice? selected = null;

	Amount({ required this.close, required this.next });

	Choice? GetChoice()
	{
		return selected;
	}

	@override
	Amount_State createState() => Amount_State();
}

class Amount_State extends State<Amount> 
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
                		padding: EdgeInsets.symmetric(horizontal: 10),
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
		      									child: ListTile
				        						(
				          							title: Text('One'),
				          							leading: Radio<Choice>
				          							(
				          								value: Choice.one,
				          								activeColor: Color(0xff152238),
				          								splashRadius: 14,
				          								groupValue: widget.selected,
				            							onChanged: (input) { setState( () { widget.selected = input; }); }
				          							)
				        						)
			        						),

			      							Expanded
			      							(
				        						child: ListTile
				        						(
				          							title: Text('More'),
				          							leading: Radio<Choice>
				          							(
				          								value: Choice.many,
				          								activeColor: Color(0xff152238),
				          								splashRadius: 14,
				          								groupValue: widget.selected,
				            							onChanged: (input) { setState( () { widget.selected = input; }); }
				          							)
				        						)
			        						)
	      								]
	      							),
      							),

      							/*Expanded(flex: 1, child: Container()),

      							Expanded
      							(
      								flex: 2,
	      							child: Row
	      							(
	      								children: <Widget>
	      								[
	      									Expanded
	      									(
	      										child: Container
	      										(
	      											color: Colors.red,
	      											alignment: Alignment.center,
							                		padding: EdgeInsets.all(2),
							                		child: TextField
							                		(
							                			onChanged: (input) { setState( () {  }); },
							                			maxLines: 1,
							                			style: TextStyle(color: Color(0xff152238)),
							                			cursorColor: Color(0xff152238),
							                			decoration: InputDecoration
							                			(
							                				contentPadding: EdgeInsets.fromLTRB(10, 0, 10, -12),
							                				enabledBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
												        	focusedBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
												        	label: Text('Type here...', style: TextStyle(color: Color(0xff152238)))
												    	),
							                		)
	      										)				
	      									),

	      									Expanded
	      									(
	      										
	      											
						                		child: TextField
						                		(
						                			onChanged: (input) { setState( () { }); },
						                			maxLines: 1,
						                			style: TextStyle(color: Color(0xff152238)),
						                			cursorColor: Color(0xff152238),
						                			decoration: InputDecoration
						                			(
						                				contentPadding: EdgeInsets.fromLTRB(10, 0, 10, -12),
						                				enabledBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
											        	focusedBorder: OutlineInputBorder(borderSide: BorderSide(width: 1, color: Color(0xff152238))),
											        	label: Text('Type here...', style: TextStyle(color: Color(0xff152238)))
											    	),
						                		)
	      										
	      									)
	      								]
	      							)
      							)*/
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
                		child: Text('Select how many chapters you need.', style: TextStyle(fontSize: 20), textAlign: TextAlign.center)
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
            				onPressed: (widget.selected != null) ? widget.next : null,
            				child: Text('Next'),
          				),
                	)
                ),
            ]
		);
	}
}