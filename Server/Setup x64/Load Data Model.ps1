# Configuration
$projectRoot = "C:\Program Files\Teraque\Credit Card Exchange"
$scriptLoader = "${projectRoot}\Web Service\FluidTrade.ScriptLoader.exe"

# Constants
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Configuration.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\AccessRight.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\CommissionType.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\CommissionUnit.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Condition.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\HolidayType.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\LotHandling.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\OrderType.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\PartyType.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\PaymentMethodType.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\PositionType.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\ReportType.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\GroupType.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\SettlementUnit.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\State.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Status.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Side.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Crossing.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\TimeInForce.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\TimeUnit.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Volume Category.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Weekend.xml"

# Data
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Image.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Type.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Crossing Manager.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\TypeTree.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Country.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Province.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Exchange.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Administrator.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Currency.xml"
 
# Reports
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Report.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\AssetViewerTemplate.xml"

# Sample Organizations
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Global Settlements Rule.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Global Settlements Schedule.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Global Settlements.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Global Settlements User.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Global Settlements Desk.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\Global Settlements Folder.xml"

&"${scriptLoader}" -i "${projectRoot}\Database\Data\National Holdings Rule.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\National Holdings Schedule.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\National Holdings.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\National Holdings User.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\National Holdings Desk.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Data\National Holdings Folder.xml"

# Generated Consumer Debt Data
&"${scriptLoader}" -i "${projectRoot}\Database\Unit Test\Consumer.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Unit Test\Credit Card.xml"
&"${scriptLoader}" -i "${projectRoot}\Database\Unit Test\Consumer Debt.xml" 
&"${scriptLoader}" -i "${projectRoot}\Database\Unit Test\Consumer Trust.xml"

# Debt Holder Orgs:
&"${scriptLoader}" -i "${projectRoot}\Database\Unit Test\National Holdings Orders.xml"

# Debt Negotiator Orgs:
&"${scriptLoader}" -i "${projectRoot}\Database\Unit Test\Global Settlements Orders.xml"