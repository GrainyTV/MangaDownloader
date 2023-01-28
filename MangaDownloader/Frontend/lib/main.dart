import 'package:flutter/material.dart';
import 'panel.dart';

void main() => runApp
(
    MaterialApp
    (
        home: HomeScreen()
    )
);

class HomeScreen extends StatefulWidget
{
    @override
    HomeScreen_State createState() => HomeScreen_State();
}

class HomeScreen_State extends State<HomeScreen>
{
    int color = 0xff152238;
    String img = 'https://cdn.myanimelist.net/s/common/uploaded_files/1673116629-e92d34f58be3fc9d28dc6daf043897ca.jpeg';
    Phase phase = Phase.main;

    @override
    Widget build(BuildContext context)
    {
        return Scaffold
        (
            appBar: AppBar
            (
                title: FittedBox
                (
                    fit: BoxFit.scaleDown,
                    child: Text('Manga Downloader', style: TextStyle(fontSize: 22, fontFamily: 'Tisa'))
                ),
                backgroundColor: Color(color)
            ),

            body: Stack
            (
                children: <Widget>
                [
                    SizedBox.expand
                    (
                        child: Image.network('$img', fit: BoxFit.fill) 
                    ),

                    Column
                    (
                        mainAxisAlignment: MainAxisAlignment.center,
                        children: <Widget>
                        [
                            Expanded(flex: 1, child: Container()),
                            Expanded(flex: 4, child: Row
                            (
                                mainAxisAlignment: MainAxisAlignment.center,
                                children: <Widget>
                                [
                                    Expanded(flex: 3, child: Container()),
                                    Expanded(flex: 2, child: Panel(phase: phase, close: () { setState( () { phase = Phase.main; }); })),
                                    Expanded(flex: 3, child: Container())
                                ]
                            )),
                            Expanded(flex: 1, child: Container())
                        ]
                    )
                ]
            ),

            floatingActionButton: FloatingActionButton.extended
            (
                onPressed: (phase == Phase.main) ? () { setState( () { phase = Phase.name; }); } : () {},
                label: Text('New', style: TextStyle(fontSize: 15, fontFamily: 'Tisa')),
                icon: Icon(Icons.add),
                backgroundColor: Color(color),
            ),
        );
    }
}