# Generated Consumer Debt Data
&"FluidTrade.ScriptLoader.exe" -i "..\Unit Test\Consumer.xml"
&"FluidTrade.ScriptLoader.exe" -i "..\Unit Test\Credit Card.xml"
&"FluidTrade.ScriptLoader.exe" -i "..\Unit Test\Consumer Debt.xml"
&"FluidTrade.ScriptLoader.exe" -i "..\Unit Test\Consumer Trust.xml"

# Generated Working Orders Data per Consumer Debt Organizations 
&"FluidTrade.ScriptLoader.exe" -i "..\Unit Test\National Holdings Orders.xml"
&"FluidTrade.ScriptLoader.exe" -i "..\Unit Test\Global Settlements Orders.xml"
