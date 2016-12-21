using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.Band;
using Microsoft.Band.Tiles;
using Microsoft.Band.Tiles.Pages;

namespace BarcodeBand
{
    public class BandHelper
    {
        private IBandClient _client;

        private Guid _pageId;

        private BandTile _tile;

        private readonly Guid _tileguid = Guid.Parse ("e38d657d-f8db-418c-8054-ab1d89db3a34");

        private readonly Guid _pageguid = Guid.Parse ("42028d00-e9a3-482e-93a3-d65e690bd1c2");

        private BandHelper () { }

        public static async Task<BandHelper> GetBandHelperAsync () {
            var bh = new BandHelper ();

            var pairedBands = await BandClientManager.Instance.GetBandsAsync ();
            bh._client = await BandClientManager.Instance.ConnectAsync (pairedBands[0]);

            return bh;
        }

        public async Task<bool> CheckIfTileAlreadyInstalledAsync () {
            var token = new CancellationToken ();

            var isinstalled = _client.TileManager.TileInstalledAndOwned (_tileguid, token);

            if (isinstalled) {
                Debug.WriteLine ("** Already installed");
                var installedtiles = await _client.TileManager.GetTilesAsync ();
                _tile = installedtiles.First (d => d.TileId == _tileguid);
            }

            return isinstalled;
        }

        public async Task<BandTile> AddNewTileAsync () {
            Debug.WriteLine ("** Not installed");
            var smallicon = await GetBandIconAsync ("ms-appx:///BarCode-WF24.png");
            var bigicon = await GetBandIconAsync ("ms-appx:///BarCode-WF48.png");

            var tile = new BandTile (_tileguid) {
                Name = "BandBarcode",
                SmallIcon = smallicon,
                TileIcon = bigicon
            };

            AddPage (tile);

            if (await _client.TileManager.AddTileAsync (tile))
                return tile;

            throw new Exception ("Error adding tile");
        }

        public void AddPage (BandTile tile) {
            var panel = new ScrollFlowPanel {
                Rect = new PageRect (0, 0, 235, 110),
                Orientation = FlowPanelOrientation.Vertical
            };

            // Movie Name
            panel.Elements.Add (new WrappedTextBlock {
                ElementId = (short?)PageElementEnum.MovieName,
                ColorSource = ElementColorSource.BandHighlight,
                Rect = new PageRect (0, 0, 235, 30),
                AutoHeight = true,
            });

            // Movie time
            panel.Elements.Add (new WrappedTextBlock {
                ElementId = (short?)PageElementEnum.MovieTime,
                ColorSource = ElementColorSource.BandSecondaryText,
                Font = WrappedTextBlockFont.Small,
                Rect = new PageRect (0, 0, 235, 30),
                AutoHeight = true
            });

            // Barcode
            panel.Elements.Add (new Barcode (BarcodeType.Code39) {
                ElementId = (short?)PageElementEnum.MovieBarcode,
                Rect = new PageRect (0, 0, 235, 130)
            });

            var pagelayout = new PageLayout (panel);

            tile.PageLayouts.Add (pagelayout);
        }

        public async Task<bool> SetPageDataAsync (Movie movie) {
            var barcodedata = new BarcodeData (BarcodeType.Code39, (short)PageElementEnum.MovieBarcode, movie.Id + movie.Time.ToString ("yyyyMMddHHmmss").Substring (5));
            var namedata = new WrappedTextBlockData ((short)PageElementEnum.MovieName, movie.Title);
            var timedata = new WrappedTextBlockData ((short)PageElementEnum.MovieTime, movie.Time.ToString ("t"));

            var pagedata = new PageData (_pageguid, 0, barcodedata, namedata, timedata);

            return await _client.TileManager.SetPagesAsync (_tileguid, pagedata);
        }

        private async Task<BandIcon> GetBandIconAsync (string iconurl) {
            var image = await StorageFile.GetFileFromApplicationUriAsync (new Uri (iconurl, UriKind.Absolute));

            using (var stream = await image.OpenAsync (FileAccessMode.Read)) {
                var bitmap = new WriteableBitmap (1, 1);
                await bitmap.SetSourceAsync (stream);
                return bitmap.ToBandIcon ();
            }
        }
    }
}
