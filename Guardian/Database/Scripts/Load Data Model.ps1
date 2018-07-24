# Configuration
$projectRoot = $(split-path -parent $(split-path -parent $(split-path -parent $(split-path -parent $SCRIPT:MyInvocation.MyCommand.Path))))
$scriptLoader = "${projectRoot}\Guardian\Script Loader\bin\Release\FluidTrade.ScriptLoader.exe"

# Constants
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Configuration.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\AccessRight.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\CommissionType.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\CommissionUnit.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Condition.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\HolidayType.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\LotHandling.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\OrderType.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\PartyType.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\PaymentMethodType.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\PositionType.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\ReportType.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\GroupType.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\SettlementUnit.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\State.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Status.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Side.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Crossing.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\TimeInForce.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\TimeUnit.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Volume Category.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Weekend.xml"

# Data
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Image.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Type.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Crossing Manager.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\TypeTree.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Country.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Province.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Exchange.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Administrator.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Currency.xml"
 
# Reports
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Report.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\AssetViewerTemplate.xml"

# Sample Organizations
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Global Settlements Rule.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Global Settlements Schedule.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Global Settlements.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Global Settlements User.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Global Settlements Desk.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\Global Settlements Folder.xml"

&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\National Holdings Rule.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\National Holdings Schedule.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\National Holdings.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\National Holdings User.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\National Holdings Desk.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Data\National Holdings Folder.xml"

# Generated Consumer Debt Data
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Unit Test\Consumer.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Unit Test\Credit Card.xml"
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Unit Test\Consumer Debt.xml" 
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Unit Test\Consumer Trust.xml"

# Debt Holder Orgs:
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Unit Test\National Holdings Orders.xml"

# Debt Negotiator Orgs:
&"${scriptLoader}" -i "${projectRoot}\Guardian\Database\Unit Test\Global Settlements Orders.xml"